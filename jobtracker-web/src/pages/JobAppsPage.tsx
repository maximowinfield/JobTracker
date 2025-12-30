import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";
import type { ApplicationStatus, JobAppDto } from "../api/jobApps";
import { createJobApp, deleteJobApp, listJobApps, updateJobApp } from "../api/jobApps";

const PAGE_SIZES = [10, 25, 50, 100] as const;

/**
 * UPDATE THIS LIST to match your backend enum values exactly.
 * api/JobTracker.Api/Models/ApplicationStatus.cs
 */
const STATUS_OPTIONS: ApplicationStatus[] = [
  "Draft",
  "Applied",
  "Interviewing",
  "Offer",
  "Rejected",
  "Accepted",
];

type FormState = {
  company: string;
  roleTitle: string;
  status: ApplicationStatus;
  notes: string;
};

function emptyForm(): FormState {
  return { company: "", roleTitle: "", status: "Draft", notes: "" };
}

function formatUtc(iso: string) {
  const d = new Date(iso);
  return isNaN(d.getTime()) ? iso : d.toLocaleString();
}

// ✅ type guard to safely narrow <select> values to ApplicationStatus
function isApplicationStatus(value: string): value is ApplicationStatus {
  return (STATUS_OPTIONS as readonly string[]).includes(value);
}

export default function JobAppsPage() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const [q, setQ] = useState("");
  const [status, setStatus] = useState<ApplicationStatus | "">("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<(typeof PAGE_SIZES)[number]>(25);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [items, setItems] = useState<JobAppDto[]>([]);
  const [total, setTotal] = useState(0);

  // modal / form state
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<JobAppDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm());
  const [saving, setSaving] = useState(false);

  const totalPages = useMemo(
    () => Math.max(1, Math.ceil(total / pageSize)),
    [total, pageSize]
  );

  async function load() {
    setLoading(true);
    setError(null);

    try {
      const res = await listJobApps({
        q: q.trim() ? q.trim() : undefined,
        status: status || undefined,
        page,
        pageSize,
      });

      setItems(res.items);
      setTotal(res.total);
    } catch (err: unknown) {
      const maybeAxiosErr = err as { response?: { status?: number } } | null;
      const code = maybeAxiosErr?.response?.status;

      if (code === 401) {
        logout();
        navigate("/login", { replace: true });
        return;
      }

      setError("Failed to load job applications.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [q, status, page, pageSize]);

  function openCreate() {
    setEditing(null);
    setForm(emptyForm());
    setShowModal(true);
  }

  function openEdit(row: JobAppDto) {
    setEditing(row);
    setForm({
      company: row.company ?? "",
      roleTitle: row.roleTitle ?? "",
      status: row.status,
      notes: row.notes ?? "",
    });
    setShowModal(true);
  }

  function closeModal() {
    setShowModal(false);
    setEditing(null);
    setSaving(false);
  }

  async function onSave() {
    const company = form.company.trim();
    const roleTitle = form.roleTitle.trim();

    if (!company || !roleTitle) {
      setError("Company and Role Title are required.");
      return;
    }

    setSaving(true);
    setError(null);

    try {
      if (editing) {
        await updateJobApp(editing.id, {
          company,
          roleTitle,
          status: form.status,
          notes: form.notes ? form.notes : null,
        });
      } else {
        await createJobApp({
          company,
          roleTitle,
          status: form.status,
          notes: form.notes ? form.notes : null,
        });
      }

      closeModal();
      await load();
    } catch (err: unknown) {
      const maybeAxiosErr = err as { response?: { status?: number } } | null;
      const code = maybeAxiosErr?.response?.status;

      if (code === 401) {
        logout();
        navigate("/login", { replace: true });
        return;
      }

      setError(
        "Save failed. (If this happened after selecting a Status, your enum values may not match.)"
      );
    } finally {
      setSaving(false);
    }
  }

  async function onDelete(row: JobAppDto) {
    const ok = window.confirm(`Delete "${row.company} — ${row.roleTitle}"?`);
    if (!ok) return;

    setError(null);
    try {
      await deleteJobApp(row.id);

      // if we deleted the last item on the page, step back
      const willHave = items.length - 1;
      if (willHave <= 0 && page > 1) setPage(page - 1);
      else await load();
    } catch (err: unknown) {
      const maybeAxiosErr = err as { response?: { status?: number } } | null;
      const code = maybeAxiosErr?.response?.status;

      if (code === 401) {
        logout();
        navigate("/login", { replace: true });
        return;
      }

      setError("Delete failed.");
    }
  }

  function applyFilters() {
    setPage(1);
    void load();
  }

  return (
    <div style={{ maxWidth: 1100, margin: "24px auto", padding: 16 }}>
      {/* Header */}
      <header
        style={{
          display: "flex",
          justifyContent: "space-between",
          gap: 12,
          alignItems: "center",
        }}
      >
        <div>
          <h1 style={{ margin: 0 }}>Job Applications</h1>
          <div style={{ opacity: 0.75, marginTop: 6 }}>
            {loading ? "Loading…" : `${total} total`}
          </div>
        </div>

        <div style={{ display: "flex", gap: 8 }}>
          <button
            onClick={openCreate}
            style={{ padding: "10px 12px", cursor: "pointer" }}
          >
            + New
          </button>
          <button
            onClick={() => {
              logout();
              navigate("/login", { replace: true });
            }}
            style={{ padding: "10px 12px", cursor: "pointer" }}
          >
            Sign out
          </button>
        </div>
      </header>

      {/* Filters */}
      <section
        style={{
          marginTop: 16,
          padding: 12,
          border: "1px solid rgba(0,0,0,0.12)",
          borderRadius: 10,
          display: "grid",
          gridTemplateColumns: "1fr 220px 140px 120px",
          gap: 10,
          alignItems: "end",
        }}
      >
        <label style={{ display: "grid", gap: 6 }}>
          Search (company, role, notes)
          <input
            value={q}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setQ(e.target.value)
            }
            onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
              if (e.key === "Enter") applyFilters();
            }}
            placeholder="e.g., Amazon"
            style={{ padding: 10 }}
          />
        </label>

        <label style={{ display: "grid", gap: 6 }}>
          Status
          <select
            value={status}
            onChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
              const v = e.target.value;
              setStatus(v === "" ? "" : isApplicationStatus(v) ? v : "");
              setPage(1);
            }}
            style={{ padding: 10 }}
          >
            <option value="">All</option>
            {STATUS_OPTIONS.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </label>

        <label style={{ display: "grid", gap: 6 }}>
          Page size
          <select
            value={pageSize}
            onChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
              const next = Number(e.target.value);
              if ((PAGE_SIZES as readonly number[]).includes(next)) {
                setPageSize(next as (typeof PAGE_SIZES)[number]);
                setPage(1);
              }
            }}
            style={{ padding: 10 }}
          >
            {PAGE_SIZES.map((n) => (
              <option key={n} value={n}>
                {n}
              </option>
            ))}
          </select>
        </label>

        <button onClick={applyFilters} style={{ padding: 10, cursor: "pointer" }}>
          Apply
        </button>
      </section>

      {error && <div style={{ marginTop: 12, color: "crimson" }}>{error}</div>}

      {/* Table */}
      <section style={{ marginTop: 14 }}>
        <div
          style={{
            overflowX: "auto",
            border: "1px solid rgba(0,0,0,0.12)",
            borderRadius: 10,
          }}
        >
          <table style={{ width: "100%", borderCollapse: "collapse", minWidth: 900 }}>
            <thead>
              <tr style={{ textAlign: "left", background: "rgba(0,0,0,0.04)" }}>
                <th style={{ padding: 12 }}>Company</th>
                <th style={{ padding: 12 }}>Role</th>
                <th style={{ padding: 12 }}>Status</th>
                <th style={{ padding: 12 }}>Updated</th>
                <th style={{ padding: 12, width: 220 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={5} style={{ padding: 16 }}>
                    Loading…
                  </td>
                </tr>
              ) : items.length === 0 ? (
                <tr>
                  <td colSpan={5} style={{ padding: 16 }}>
                    No results.
                  </td>
                </tr>
              ) : (
                items.map((row) => (
                  <tr key={row.id} style={{ borderTop: "1px solid rgba(0,0,0,0.08)" }}>
                    <td style={{ padding: 12 }}>{row.company}</td>
                    <td style={{ padding: 12 }}>{row.roleTitle}</td>
                    <td style={{ padding: 12 }}>{row.status}</td>
                    <td style={{ padding: 12 }}>{formatUtc(row.updatedAtUtc)}</td>
                    <td style={{ padding: 12, display: "flex", gap: 8 }}>
                      <button
                        onClick={() => openEdit(row)}
                        style={{ padding: "8px 10px", cursor: "pointer" }}
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => onDelete(row)}
                        style={{ padding: "8px 10px", cursor: "pointer" }}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            marginTop: 12,
          }}
        >
          <div style={{ opacity: 0.75 }}>
            Page {page} of {totalPages}
          </div>

          <div style={{ display: "flex", gap: 8 }}>
            <button
              onClick={() => setPage(1)}
              disabled={page <= 1}
              style={{ padding: "8px 10px" }}
            >
              First
            </button>
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page <= 1}
              style={{ padding: "8px 10px" }}
            >
              Prev
            </button>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page >= totalPages}
              style={{ padding: "8px 10px" }}
            >
              Next
            </button>
            <button
              onClick={() => setPage(totalPages)}
              disabled={page >= totalPages}
              style={{ padding: "8px 10px" }}
            >
              Last
            </button>
          </div>
        </div>
      </section>

      {/* Modal */}
      {showModal && (
        <div
          role="dialog"
          aria-modal="true"
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(0,0,0,0.45)",
            display: "grid",
            placeItems: "center",
            padding: 16,
          }}
          onMouseDown={(e: React.MouseEvent<HTMLDivElement>) => {
            if (e.target === e.currentTarget) closeModal();
          }}
        >
          <div style={{ width: "100%", maxWidth: 560, background: "#fff", borderRadius: 12, padding: 16 }}>
            <h2 style={{ marginTop: 0 }}>{editing ? "Edit Application" : "New Application"}</h2>

            <div style={{ display: "grid", gap: 10 }}>
              <label style={{ display: "grid", gap: 6 }}>
                Company
                <input
                  value={form.company}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setForm((p) => ({ ...p, company: e.target.value }))
                  }
                  style={{ padding: 10 }}
                />
              </label>

              <label style={{ display: "grid", gap: 6 }}>
                Role Title
                <input
                  value={form.roleTitle}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setForm((p) => ({ ...p, roleTitle: e.target.value }))
                  }
                  style={{ padding: 10 }}
                />
              </label>

              <label style={{ display: "grid", gap: 6 }}>
                Status
                <select
                  value={form.status}
                  onChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
                    const v = e.target.value;
                    if (isApplicationStatus(v)) {
                      setForm((p) => ({ ...p, status: v }));
                    }
                  }}
                  style={{ padding: 10 }}
                >
                  {STATUS_OPTIONS.map((s) => (
                    <option key={s} value={s}>
                      {s}
                    </option>
                  ))}
                </select>
              </label>

              <label style={{ display: "grid", gap: 6 }}>
                Notes
                <textarea
                  value={form.notes}
                  onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
                    setForm((p) => ({ ...p, notes: e.target.value }))
                  }
                  rows={5}
                  style={{ padding: 10, resize: "vertical" }}
                />
              </label>
            </div>

            <div style={{ display: "flex", justifyContent: "flex-end", gap: 8, marginTop: 14 }}>
              <button onClick={closeModal} disabled={saving} style={{ padding: "10px 12px", cursor: "pointer" }}>
                Cancel
              </button>
              <button onClick={onSave} disabled={saving} style={{ padding: "10px 12px", cursor: "pointer" }}>
                {saving ? "Saving…" : "Save"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
