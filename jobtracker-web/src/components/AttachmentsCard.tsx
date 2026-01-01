import { useEffect, useMemo, useState } from "react";

type AttachmentDto = {
  id: number;
  jobApplicationId: number;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  storageKey: string;
  createdAtUtc: string;
};

type PresignUploadResponse = {
  uploadUrl: string;
  storageKey: string;
  expiresInSeconds: number;
};

type PresignDownloadResponse = {
  downloadUrl: string;
  expiresInSeconds: number;
};

function formatBytes(bytes: number): string {
  if (!Number.isFinite(bytes) || bytes < 0) return `${bytes}`;
  const units = ["B", "KB", "MB", "GB"];
  let v = bytes;
  let i = 0;
  while (v >= 1024 && i < units.length - 1) {
    v = v / 1024;
    i++;
  }
  return `${v.toFixed(i === 0 ? 0 : 1)} ${units[i]}`;
}

function formatDate(iso: string): string {
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? iso : d.toLocaleString();
}

export default function AttachmentsCard(props: {
  jobAppId: number;
  apiBaseUrl: string;
  token: string;
}) {
  const { jobAppId, apiBaseUrl, token } = props;

  const base = apiBaseUrl.replace(/\/+$/, "");

  const [attachments, setAttachments] = useState<AttachmentDto[]>([]);
  const [loadingList, setLoadingList] = useState(false);

  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [status, setStatus] = useState<string>("");

  const maxBytes = 25 * 1024 * 1024;

  const canUpload = useMemo(() => !!file && !uploading, [file, uploading]);

async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${base}${path}`, {
    ...init,
    headers: {
      ...(init?.headers ?? {}),
      Authorization: `Bearer ${token}`,
    },
  });


    if (!res.ok) {
      const text = await res.text().catch(() => "");
      throw new Error(`API ${res.status}: ${text || res.statusText}`);
    }
    return (await res.json()) as T;
  }

  async function loadAttachments() {
    setLoadingList(true);
    try {
      const data = await apiFetch<AttachmentDto[]>(
        `/job-apps/${jobAppId}/attachments`
      );
      setAttachments(data);
    } finally {
      setLoadingList(false);
    }
  }

  useEffect(() => {
    void loadAttachments();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [jobAppId]);

  async function uploadSelectedFile() {
    if (!file) return;

    // basic client validation
    if (file.size <= 0) {
      setStatus("That file looks empty.");
      return;
    }
    if (file.size > maxBytes) {
      setStatus("File too large (max 25 MB).");
      return;
    }

    setUploading(true);
    setStatus("Presigning upload…");

    try {
      // 1) presign
      const presign = await apiFetch<PresignUploadResponse>(
        `/job-apps/${jobAppId}/attachments/presign-upload`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            fileName: file.name,
            contentType: file.type || "application/octet-stream",
            sizeBytes: file.size,
          }),
        }
      );

      // 2) PUT to S3 (no Authorization header)
      setStatus("Uploading to S3…");
      const putRes = await fetch(presign.uploadUrl, {
        method: "PUT",
        headers: {
          "Content-Type": file.type || "application/octet-stream",
        },
        body: file,
      });

      if (!putRes.ok) {
        const text = await putRes.text().catch(() => "");
        throw new Error(`S3 PUT ${putRes.status}: ${text || putRes.statusText}`);
      }

      // 3) save metadata
      setStatus("Saving attachment…");
      await apiFetch<AttachmentDto>(`/api/job-apps/${jobAppId}/attachments`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          fileName: file.name,
          contentType: file.type || "application/octet-stream",
          sizeBytes: file.size,
          storageKey: presign.storageKey,
        }),
      });

      setStatus("Uploaded ✅");
      setFile(null);
      await loadAttachments();
    } catch (e) {
      const msg = e instanceof Error ? e.message : String(e);
      setStatus(msg);
    } finally {
      setUploading(false);
      // optional: clear status after a bit
      // setTimeout(() => setStatus(""), 2500);
    }
  }

  async function downloadAttachment(attachmentId: number) {
    try {
      const data = await apiFetch<PresignDownloadResponse>(
        `/attachments/${attachmentId}/presign-download`
      );
      window.open(data.downloadUrl, "_blank", "noopener,noreferrer");
    } catch (e) {
      const msg = e instanceof Error ? e.message : String(e);
      setStatus(msg);
    }
  }

  async function deleteAttachment(attachmentId: number) {
    if (!confirm("Delete this attachment?")) return;
    try {
      await apiFetch<void>(`/attachments/${attachmentId}`, { method: "DELETE" });
      setStatus("Deleted ✅");
      await loadAttachments();
    } catch (e) {
      const msg = e instanceof Error ? e.message : String(e);
      setStatus(msg);
    }
  }

return (
  <div className="rounded-xl border border-slate-200 bg-white p-3">
    <div className="flex items-center justify-between">
      <h3 className="m-0 text-sm font-semibold text-slate-900">Attachments</h3>
      <span className="text-xs text-slate-500">Max: {formatBytes(maxBytes)}</span>
    </div>

    {/* Upload row */}
    <div className="mt-3 flex flex-col gap-2 sm:flex-row sm:items-center">
      <input
        type="file"
        onChange={(e) => setFile(e.target.files?.[0] ?? null)}
        disabled={uploading}
        className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm outline-none
                   focus:border-slate-300 focus:ring-2 focus:ring-slate-200 disabled:cursor-not-allowed disabled:opacity-50
                   file:mr-3 file:rounded-lg file:border file:border-slate-200 file:bg-white
                   file:px-3 file:py-2 file:text-sm file:font-medium file:text-slate-900
                   hover:file:bg-slate-50"
      />

      <button
        onClick={() => void uploadSelectedFile()}
        disabled={!canUpload}
        className="rounded-lg bg-slate-900 px-3 py-2 text-sm font-semibold text-white
                   enabled:hover:bg-slate-800 active:bg-slate-950 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {uploading ? "Uploading…" : "Upload"}
      </button>
    </div>

    {/* Selected file */}
    {file && (
      <div className="mt-2 text-xs text-slate-500">
        Selected: <span className="font-semibold text-slate-900">{file.name}</span>{" "}
        ({formatBytes(file.size)})
      </div>
    )}

    {/* Status */}
    {status && (
      <div className="mt-2 text-sm text-slate-700">
        {status}
      </div>
    )}

    {/* List */}
    <div className="mt-3">
      {loadingList ? (
        <div className="text-sm text-slate-500">Loading attachments…</div>
      ) : attachments.length === 0 ? (
        <div className="text-sm text-slate-500">No attachments yet.</div>
      ) : (
        <div className="grid gap-2">
          {attachments.map((a) => (
            <div
              key={a.id}
              className="flex items-center justify-between gap-3 rounded-xl border border-slate-200 bg-white p-3"
            >
              <div className="min-w-0">
                <div className="truncate text-sm font-semibold text-slate-900">{a.fileName}</div>
                <div className="text-xs text-slate-500">
                  {formatBytes(a.sizeBytes)} • {formatDate(a.createdAtUtc)}
                </div>
              </div>

              <div className="flex shrink-0 gap-2">
                <button
                  onClick={() => void downloadAttachment(a.id)}
                  className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium
                             hover:bg-slate-50 active:bg-slate-100 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Download
                </button>

                <button
                  onClick={() => void deleteAttachment(a.id)}
                  className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium
                             hover:bg-slate-50 active:bg-slate-100 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  </div>
);
}