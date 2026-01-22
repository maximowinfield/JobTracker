import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import toast from "react-hot-toast";
import type React from "react";
import { createJobApp, deleteJobApp, listJobApps, updateJobApp } from "../api/jobApps";
import type { ApplicationStatus, JobAppDto } from "../api/jobApps";
import AttachmentsCard from "../components/AttachmentsCard";
import StatusBadge from "../components/StatusBadge";
import { useAuth } from "../context/useAuth";

import {
  DndContext,
  DragOverlay,
  KeyboardSensor,
  PointerSensor,
  pointerWithin,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import {
  SortableContext,
  verticalListSortingStrategy,
  useSortable,
  sortableKeyboardCoordinates,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { useDroppable } from "@dnd-kit/core";

/**
 * JobAppsPage (Core UI)
 * - Purpose: This is the main "product" screen.
 *   It supports:
 *   1) Viewing job applications as a Kanban board (drag-and-drop lanes)
 *   2) Viewing job applications as an Archive table (filter/paginate/manage)
 *   3) Creating, editing, deleting applications (modal form)
 *   4) Keeping filters in sync with the URL (shareable + refresh-safe)
 *
 * Interview highlights (what to focus on):
 * - Data loading + paging/filtering (load + query params)
 * - Kanban drag-and-drop -> status update (DndContext + moveToLane)
 * - Optimistic UI update with rollback on failure (moveToLane)
 */

const PAGE_SIZES = [10, 25, 50, 100] as const;

/**
 * IMPORTANT:
 * Frontend enum strings must match backend enum names EXACTLY.
 * This prevents "invalid status" errors when PATCHing status.
 */
const STATUS_OPTIONS: ApplicationStatus[] = [
  "Draft",
  "Applied",
  "Interviewing",
  "Offer",
  "Rejected",
  "Accepted",
];

// BoardLane is the subset of statuses we render as Kanban columns.
// (Archive can still show the other statuses like Rejected/Accepted.)
type BoardLane = "Draft" | "Applied" | "Interviewing" | "Offer";
type ViewMode = "Board" | "Archive";

const BOARD_LANES: BoardLane[] = ["Draft", "Applied", "Interviewing", "Offer"];

// UI label mapping: Draft is displayed as "Wishlist" on the board.
function laneLabel(status: BoardLane) {
  return status === "Draft" ? "Wishlist" : status;
}

// Form state for the create/edit modal
type FormState = {
  company: string;
  roleTitle: string;
  status: ApplicationStatus;
  notes: string;
};

function emptyForm(): FormState {
  return { company: "", roleTitle: "", status: "Draft", notes: "" };
}

// Display helper for UTC timestamps returned by the API
function formatUtc(iso: string) {
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? iso : d.toLocaleString();
}

/**
 * Type guard to safely narrow <select> values to ApplicationStatus.
 * Interview talking point: prevents invalid strings from entering state.
 */
function isApplicationStatus(value: string): value is ApplicationStatus {
  return (STATUS_OPTIONS as readonly string[]).includes(value);
}

function parseIntOr(value: string | null, fallback: number): number {
  const n = Number(value);
  return Number.isFinite(n) && n > 0 ? n : fallback;
}

/**
 * DnD helpers
 * - We encode card IDs as strings so DnD kit can handle them consistently.
 */
function cardId(appId: number) {
  return `job:${appId}`;
}

function isCardId(id: string) {
  return id.startsWith("job:");
}

function parseCardId(id: string) {
  return Number(id.replace("job:", ""));
}

/**
 * Safety: response shape guard (helps if API returns unexpected data).
 * Interview talking point: defensive parsing avoids UI crashes.
 */
type JobAppsResponseShape = {
  items?: unknown;
  total?: unknown;
};

/**
 * LaneDroppable
 * - Purpose: A lane (column) that can receive dropped cards.
 * - Interview talking point: We attach DnD metadata `{ type: "lane" }` so we can
 *   distinguish lane drops from card drops.
 */
function LaneDroppable({
  id,
  children,
  className,
}: {
  id: string;
  children: React.ReactNode;
  className: string;
}) {
  const { setNodeRef, isOver } = useDroppable({
    id,
    data: { type: "lane" as const },
  });

  // Visual feedback when dragging over a lane
  return (
    <div
      ref={setNodeRef}
      className={[
        className,
        "transition-colors duration-150",
        isOver ? "bg-slate-100 ring-2 ring-slate-300" : "bg-slate-50",
      ].join(" ")}
    >
      {children}
    </div>
  );
}

/**
 * SortableJobCard
 * - Purpose: A draggable Kanban card.
 * - Interview talking point: click-to-edit is disabled while dragging to avoid accidental opens.
 */
function SortableJobCard({
  id,
  company,
  roleTitle,
  updatedAtUtc,
  onClick,
}: {
  id: string;
  company: string;
  roleTitle: string;
  updatedAtUtc: string;
  onClick: () => void;
}) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id,
    data: { type: "card" as const },
  });

  // DnD kit provides transform/transition for smooth dragging
  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <button
      ref={setNodeRef}
      style={style}
      type="button"
      onClick={() => {
        // Prevent opening the modal when the user intended to drag
        if (isDragging) return;
        onClick();
      }}
      className={[
        "w-full rounded-2xl border border-slate-200 bg-white p-3 text-left shadow-sm hover:bg-slate-50",
        isDragging ? "opacity-60 ring-2 ring-slate-300" : "",
      ].join(" ")}
      {...attributes}
      {...listeners}
    >
      <div className="truncate font-semibold text-slate-900">{company}</div>
      <div className="truncate text-sm text-slate-600">{roleTitle}</div>
      <div className="mt-2 text-xs text-slate-500">Updated {updatedAtUtc}</div>
    </button>
  );
}

export default function JobAppsPage() {
  const { logout, token } = useAuth();
  const apiBaseUrl = (import.meta.env.VITE_API_URL as string | undefined) ?? "/api";

  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  /**
   * Search: debounced input
   * - qInput: what user is typing
   * - qCommitted: what we actually use for API + URL
   *
   * Interview talking point:
   * This avoids calling the API on every keystroke and makes the URL shareable.
   */
  const [qInput, setQInput] = useState("");
  const [qCommitted, setQCommitted] = useState("");
  const debounceRef = useRef<number | null>(null);

  // Track focus so we don't commit/reload mid-edit and disrupt typing
  const searchInputRef = useRef<HTMLInputElement | null>(null);
  const [isSearchFocused, setIsSearchFocused] = useState(false);

  // Toast handling (avoid flashing loading toasts for quick loads)
  const loadToastIdRef = useRef<string | null>(null);
  const loadToastTimerRef = useRef<number | null>(null);

  /**
   * Load sequencing
   * - Purpose: prevent stale responses from overwriting newer results.
   * - Interview talking point: classic "request race" guard.
   */
  const loadSeq = useRef(0);

  function clearLoadToastTimer() {
    if (loadToastTimerRef.current) {
      window.clearTimeout(loadToastTimerRef.current);
      loadToastTimerRef.current = null;
    }
  }

  function dismissLoadToast() {
    if (loadToastIdRef.current) {
      toast.dismiss(loadToastIdRef.current);
      loadToastIdRef.current = null;
    }
  }

  // Filters + paging
  const [status, setStatus] = useState<ApplicationStatus | "">("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<(typeof PAGE_SIZES)[number]>(25);

  // Fetch state
  const [loading, setLoading] = useState(true);
  const [isFetching, setIsFetching] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Data
  const [items, setItems] = useState<JobAppDto[]>([]);
  const [total, setTotal] = useState(0);

  // UI view mode
  const [view, setView] = useState<ViewMode>("Board");

  /**
   * DnD Sensors
   * - PointerSensor: mouse/touch dragging
   * - KeyboardSensor: accessibility + keyboard-based movement
   * - activationConstraint: reduces accidental drags (user must move 6px)
   */
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 6 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })
  );

  // Active drag overlay state
  const [activeId, setActiveId] = useState<string | null>(null);

  // Derived active job (used by DragOverlay preview)
  const activeJob = useMemo(() => {
    if (!activeId || !isCardId(activeId)) return null;
    const jobId = parseCardId(activeId);
    return items.find((x) => x.id === jobId) ?? null;
  }, [activeId, items]);

  // Modal / form state
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<JobAppDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm());
  const [saving, setSaving] = useState(false);

  // Delete confirm modal state
  const [confirmDelete, setConfirmDelete] = useState<JobAppDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  const totalPages = useMemo(() => Math.max(1, Math.ceil(total / pageSize)), [total, pageSize]);

  /**
   * URL -> State initialization (runs once)
   * Interview talking point:
   * - Supports refresh/share: query params restore filters/paging.
   */
  useEffect(() => {
    const urlQ = searchParams.get("q") ?? "";
    const urlStatus = searchParams.get("status") ?? "";
    const urlPage = parseIntOr(searchParams.get("page"), 1);
    const urlPageSize = parseIntOr(searchParams.get("pageSize"), 25);

    setQCommitted(urlQ);
    setQInput(urlQ);

    setStatus(urlStatus === "" ? "" : isApplicationStatus(urlStatus) ? urlStatus : "");
    setPage(urlPage);

    const sizeOk = (PAGE_SIZES as readonly number[]).includes(urlPageSize);
    setPageSize(sizeOk ? (urlPageSize as (typeof PAGE_SIZES)[number]) : 25);

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /**
   * Debounce qInput -> qCommitted
   * - Only commits while focused to prevent "kicking" focus from re-renders mid-edit.
   * - 400ms delay is a practical compromise for UX + performance.
   */
  useEffect(() => {
    if (debounceRef.current) {
      window.clearTimeout(debounceRef.current);
      debounceRef.current = null;
    }

    debounceRef.current = window.setTimeout(() => {
      if (!isSearchFocused) return;

      const next = qInput.trim();
      setQCommitted((prev) => (prev === next ? prev : next));

      // reset paging when the query changes
      setPage(1);
    }, 400);

    return () => {
      if (debounceRef.current) {
        window.clearTimeout(debounceRef.current);
        debounceRef.current = null;
      }
    };
  }, [qInput, isSearchFocused]);

  /**
   * State -> URL synchronization
   * Interview talking point:
   * - Keeps UI state shareable and refresh-safe.
   * - replace:true avoids creating a new history entry on every filter change.
   */
  useEffect(() => {
    const next = new URLSearchParams();

    if (qCommitted) next.set("q", qCommitted);
    if (status) next.set("status", status);
    if (page !== 1) next.set("page", String(page));
    if (pageSize !== 25) next.set("pageSize", String(pageSize));

    setSearchParams(next, { replace: true });
  }, [qCommitted, status, page, pageSize, setSearchParams]);

  /**
   * Board mode tuning
   * - Board view wants broader data so lanes populate well.
   * - We remove status filter, reset page, and bump pageSize to 100.
   */
  useEffect(() => {
    if (view !== "Board") return;

    if (status !== "") setStatus("");
    if (page !== 1) setPage(1);
    const boardPageSize = PAGE_SIZES[PAGE_SIZES.length - 1]; // 100
    if (pageSize !== boardPageSize) setPageSize(boardPageSize);

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [view]);

  /**
   * load (Data Fetch)
   * Interview talking point:
   * - Handles paging/filter/search
   * - Guards against request races (seq)
   * - Handles 401 by logging out + redirecting
   * - Uses "delayed loading toast" so fast loads don't flash UI noise
   */
  async function load() {
    const seq = ++loadSeq.current;
    setError(null);

    const isFirstLoad = items.length === 0 && total === 0;
    if (isFirstLoad) setLoading(true);
    else setIsFetching(true);

    clearLoadToastTimer();
    dismissLoadToast();

    loadToastTimerRef.current = window.setTimeout(() => {
      loadToastIdRef.current = toast.loading("Loading applications…");
    }, 500);

    try {
      const res = await listJobApps({
        q: qCommitted ? qCommitted : undefined,
        status: status || undefined,
        page,
        pageSize,
      });

      // Ignore stale responses (if a newer load started)
      if (seq !== loadSeq.current) return;

      const data = res as unknown as JobAppsResponseShape;

      const nextItems = Array.isArray(data.items) ? (data.items as JobAppDto[]) : [];

      const nextTotal =
        typeof data.total === "number"
          ? data.total
          : typeof data.total === "string" && Number.isFinite(Number(data.total))
            ? Number(data.total)
            : nextItems.length;

      setItems(nextItems);
      setTotal(nextTotal);
    } catch (err: unknown) {
      if (seq !== loadSeq.current) return;

      const maybeAxiosErr = err as { response?: { status?: number } } | null;
      const code = maybeAxiosErr?.response?.status;

      // Auth failure: clear token and push user back to login
      if (code === 401) {
        logout();
        navigate("/login", { replace: true });
        return;
      }

      setError("Failed to load job applications.");
      toast.error("Failed to load applications.");
    } finally {
      if (seq === loadSeq.current) {
        clearLoadToastTimer();
        dismissLoadToast();
        setLoading(false);
        setIsFetching(false);
      }
    }
  }

  // Trigger load whenever the effective query changes
  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [qCommitted, status, page, pageSize]);

  // Modal helpers
  function openCreate() {
    setEditing(null);
    setForm(emptyForm());
    setShowModal(true);
  }

  function openCreateForLane(lane: BoardLane) {
    setEditing(null);
    setForm({
      company: "",
      roleTitle: "",
      status: lane,
      notes: "",
    });
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

  function openDeleteConfirm(row: JobAppDto) {
    setConfirmDelete(row);
  }

  function closeDeleteConfirm() {
    setConfirmDelete(null);
  }

  /**
   * moveToLane (Kanban move -> Backend status update)
   * Interview talking point:
   * - Optimistic UI update for snappy UX
   * - PATCH minimal payload: { status }
   * - Rollback if request fails (preserves data integrity)
   */
  async function moveToLane(jobId: number, nextLane: BoardLane) {
    const job = items.find((x) => x.id === jobId);
    if (!job) return;

    // Optimistic UI update: user sees the move instantly
    setItems((prev) => prev.map((x) => (x.id === jobId ? { ...x, status: nextLane } : x)));

    try {
      // Minimal PATCH payload: only update what changed
      await updateJobApp(jobId, { status: nextLane });
      toast.success(`Moved to ${laneLabel(nextLane)}.`);
    } catch (err: unknown) {
      // Debug info to help diagnose enum mismatches or auth issues
      console.error("MOVE FAILED", err);

      const maybeAxiosErr = err as {
        response?: { status?: number; data?: unknown };
        message?: string;
      };

      console.error("MOVE FAILED status:", maybeAxiosErr?.response?.status);
      console.error("MOVE FAILED data:", maybeAxiosErr?.response?.data);

      // Rollback: restore the original job state if the API call fails
      requestAnimationFrame(() => {
        setItems((prev) => prev.map((x) => (x.id === jobId ? job : x)));
      });

      toast.error("Move failed.");
    }
  }

  /**
   * onSave (Create/Edit modal submit)
   * Interview talking point:
   * - Validates required fields
   * - Uses toast.promise for consistent loading/success/error UX
   * - Reloads data after write to keep list consistent with backend
   */
  async function onSave() {
    const company = form.company.trim();
    const roleTitle = form.roleTitle.trim();

    if (!company || !roleTitle) {
      setError("Company and Role Title are required.");
      toast.error("Company and Role Title are required.");
      return;
    }

    setSaving(true);
    setError(null);

    const run = async () => {
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
          throw new Error("Unauthorized");
        }

        setError(
          "Save failed. (If this happened after selecting a Status, your enum values may not match.)"
        );
        throw err; // rethrow so toast.promise triggers error toast
      } finally {
        setSaving(false);
      }
    };

    await toast.promise(run(), {
      loading: editing ? "Saving changes…" : "Creating application…",
      success: editing ? "Application updated." : "Application created.",
      error: "Save failed.",
    });
  }

  /**
   * onDelete (Delete confirmation)
   * Interview talking point:
   * - Handles paging edge case (if last item on page deleted, go back a page)
   */
  async function onDelete(row: JobAppDto) {
    setError(null);

    const run = async () => {
      try {
        await deleteJobApp(row.id);

        const willHave = items.length - 1;
        if (willHave <= 0 && page > 1) setPage(page - 1);
        else await load();
      } catch (err: unknown) {
        const maybeAxiosErr = err as { response?: { status?: number } } | null;
        const code = maybeAxiosErr?.response?.status;

        if (code === 401) {
          logout();
          navigate("/login", { replace: true });
          throw new Error("Unauthorized");
        }

        setError("Delete failed.");
        throw err;
      }
    };

    await toast.promise(run(), {
      loading: "Deleting…",
      success: "Application deleted.",
      error: "Delete failed.",
    });
  }

  return (
    <div className="min-h-screen w-full bg-neutral-900">
      <div className="mx-auto max-w-5xl p-4 md:p-6">
        {/* Header: view toggle + create + sign out */}
        <header className="flex items-center justify-between gap-3">
          <div>
            <h1 className="m-0 text-2xl font-bold">Job Applications</h1>
            <div className="mt-1 text-sm text-slate-500">
              {loading || isFetching ? "Loading…" : `${total} total`}
            </div>
          </div>

          <div className="flex items-center gap-2">
            {/* View toggle: Board vs Archive */}
            <div className="inline-flex overflow-hidden rounded-lg border border-slate-200 bg-white">
              <button
                type="button"
                onClick={() => setView("Board")}
                className={[
                  "px-3 py-2 text-sm font-medium",
                  view === "Board"
                    ? "bg-slate-900 text-white"
                    : "text-slate-700 hover:bg-slate-50",
                ].join(" ")}
              >
                Board
              </button>
              <button
                type="button"
                onClick={() => setView("Archive")}
                className={[
                  "px-3 py-2 text-sm font-medium",
                  view === "Archive"
                    ? "bg-slate-900 text-white"
                    : "text-slate-700 hover:bg-slate-50",
                ].join(" ")}
              >
                Archive
              </button>
            </div>

            <button
              onClick={openCreate}
              disabled={loading || saving}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium hover:bg-slate-50 active:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
            >
              + New
            </button>

            <button
              onClick={() => {
                // Centralized logout clears token + headers; we redirect immediately.
                logout();
                navigate("/login", { replace: true });
              }}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium hover:bg-slate-50 active:bg-slate-100"
            >
              Sign out
            </button>
          </div>
        </header>

        {/* Filters: search + status + page size (Archive primarily; Board auto-broadens) */}
        <section className="mt-4 grid grid-cols-1 gap-3 rounded-xl border border-slate-200 bg-white p-3 md:grid-cols-[1fr_220px_140px_120px] md:items-end">
          <label className="grid gap-1.5 text-sm font-medium text-slate-700">
            Search (company, role, notes)
            <input
              ref={searchInputRef}
              value={qInput}
              onChange={(e) => setQInput(e.target.value)}
              onFocus={() => setIsSearchFocused(true)}
              onBlur={() => setIsSearchFocused(false)}
              onKeyDown={(e) => {
                if (e.key !== "Enter") return;
                e.preventDefault();

                // Cancel pending debounce and commit immediately
                if (debounceRef.current) {
                  window.clearTimeout(debounceRef.current);
                  debounceRef.current = null;
                }

                const next = qInput.trim();
                setQCommitted(next);
                setPage(1);
              }}
              placeholder="e.g., Amazon"
              disabled={saving || deleting}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm outline-none focus:border-slate-300 focus:ring-2 focus:ring-slate-200 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </label>

          <label className="grid gap-1.5 text-sm font-medium text-slate-700">
            Status
            <select
              value={status}
              onChange={(e) => {
                const v = e.target.value;
                setStatus(v === "" ? "" : isApplicationStatus(v) ? v : "");
                setPage(1);
              }}
              disabled={loading}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm outline-none focus:border-slate-300 focus:ring-2 focus:ring-slate-200 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="">All</option>
              {STATUS_OPTIONS.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </label>

          <label className="grid gap-1.5 text-sm font-medium text-slate-700">
            Page size
            <select
              value={pageSize}
              onChange={(e) => {
                const next = Number(e.target.value);
                if ((PAGE_SIZES as readonly number[]).includes(next)) {
                  setPageSize(next as (typeof PAGE_SIZES)[number]);
                  setPage(1);
                }
              }}
              disabled={loading}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm outline-none focus:border-slate-300 focus:ring-2 focus:ring-slate-200 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {PAGE_SIZES.map((n) => (
                <option key={n} value={n}>
                  {n}
                </option>
              ))}
            </select>
          </label>
        </section>

        {error && <div className="mt-3 text-sm font-medium text-red-600">{error}</div>}

        {/* Board (Kanban) */}
        {view === "Board" && (
          <section className="mt-4 h-[calc(100vh-260px)]">
            <DndContext
              sensors={sensors}
              collisionDetection={pointerWithin}
              onDragStart={(e) => setActiveId(String(e.active.id))}
              onDragCancel={() => setActiveId(null)}
              /**
               * Drag End (Core Kanban Logic)
               * - Determine which card was dragged and which lane (or card) it was dropped onto.
               * - Then call moveToLane(jobId, nextLane) which PATCHes status to backend.
               */
              onDragEnd={(e) => {
                const active = String(e.active.id);
                const over = e.over ? String(e.over.id) : null;

                setActiveId(null);

                if (!over) return;
                if (!isCardId(active)) return;

                const jobId = parseCardId(active);

                // FIRST: dropped directly on a lane
                if (over.startsWith("lane:")) {
                  const nextLane = over.replace("lane:", "") as BoardLane;
                  void moveToLane(jobId, nextLane);
                  return;
                }

                // SECOND: fallback - dropped on another card; infer lane from that card
                if (isCardId(over)) {
                  const overJobId = parseCardId(over);
                  const overJob = items.find((x) => x.id === overJobId);
                  if (!overJob) return;

                  // Guard against non-board statuses (Archive-only states)
                  if (
                    overJob.status !== "Draft" &&
                    overJob.status !== "Applied" &&
                    overJob.status !== "Interviewing" &&
                    overJob.status !== "Offer"
                  ) {
                    return;
                  }

                  void moveToLane(jobId, overJob.status);
                }
              }}
            >
              <div className="grid gap-4 rounded-xl border border-slate-200 bg-white p-3 grid-cols-1 md:grid-cols-2 xl:grid-cols-4">
                {BOARD_LANES.map((lane) => {
                  const laneItems = (items ?? []).filter((x) => x.status === lane);
                  const laneDroppableId = `lane:${lane}`;
                  const sortableIds = laneItems.map((x) => cardId(x.id));

                  return (
                    <LaneDroppable
                      key={lane}
                      id={laneDroppableId}
                      className="min-w-0 rounded-2xl p-3 flex flex-col"
                    >
                      <div className="mb-3 flex items-center justify-between">
                        <div className="font-semibold text-slate-900">{laneLabel(lane)}</div>

                        <div className="flex items-center gap-2">
                          <div className="text-sm text-slate-500">{laneItems.length}</div>

                          <button
                            type="button"
                            onClick={() => openCreateForLane(lane)}
                            className="inline-flex h-7 w-7 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-700 hover:bg-slate-50 active:bg-slate-100"
                            aria-label={`Create new application in ${laneLabel(lane)}`}
                            title="Add application"
                          >
                            +
                          </button>
                        </div>
                      </div>

                      {/* Sortable list inside each lane */}
                      <SortableContext items={sortableIds} strategy={verticalListSortingStrategy}>
                        <div className="space-y-2 overflow-y-auto flex-1 min-h-[220px]">
                          {loading || isFetching ? (
                            <div className="rounded-xl border border-slate-200 bg-white p-3 text-sm text-slate-600">
                              Loading…
                            </div>
                          ) : laneItems.length === 0 ? (
                            <div className="rounded-xl border border-dashed border-slate-300 bg-white p-3 text-sm text-slate-600">
                              No applications here yet.
                            </div>
                          ) : (
                            laneItems.map((row) => (
                              <SortableJobCard
                                key={row.id}
                                id={cardId(row.id)}
                                company={row.company}
                                roleTitle={row.roleTitle}
                                updatedAtUtc={formatUtc(row.updatedAtUtc)}
                                onClick={() => openEdit(row)}
                              />
                            ))
                          )}
                        </div>
                      </SortableContext>
                    </LaneDroppable>
                  );
                })}
              </div>

              {/* DragOverlay shows a nice “floating preview” while dragging */}
              <DragOverlay>
                {activeJob ? (
                  <div className="w-[280px] rounded-2xl border border-slate-200 bg-white p-3 text-left shadow-lg">
                    <div className="truncate font-semibold text-slate-900">{activeJob.company}</div>
                    <div className="truncate text-sm text-slate-600">{activeJob.roleTitle}</div>
                  </div>
                ) : null}
              </DragOverlay>
            </DndContext>
          </section>
        )}
        {view === "Archive" && (
          <>
            {/* 
              ARCHIVE VIEW (Table)
              - Purpose: This is the “data management” view.
              - You can filter + paginate + edit + delete applications in a dense table format.
              - Interview soundbite: “Board is for workflow; Archive is for administration.”
            */}
            <section className="mt-4 h-[calc(100vh-260px)]">
              <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
                <table className="w-full min-w-[900px] border-collapse text-left text-sm">
                  <thead>
                    <tr className="bg-slate-50">
                      <th className="px-3 py-3 font-semibold text-slate-700">Company</th>
                      <th className="px-3 py-3 font-semibold text-slate-700">Role</th>
                      <th className="px-3 py-3 font-semibold text-slate-700">Status</th>
                      <th className="px-3 py-3 font-semibold text-slate-700">Updated</th>
                      <th className="px-3 py-3 font-semibold text-slate-700">Actions</th>
                    </tr>
                  </thead>

                  <tbody>
                    {loading || isFetching ? (
                      <tr>
                        <td colSpan={5} className="px-3 py-4 text-slate-600">
                          Loading…
                        </td>
                      </tr>
                    ) : items.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="px-3 py-6">
                          <div className="grid gap-2">
                            <div className="font-semibold text-slate-800">
                              {qCommitted || status ? "No matching results" : "No applications yet"}
                            </div>
                            <div className="text-slate-500">
                              {qCommitted || status
                                ? "Try clearing filters or searching a different keyword."
                                : "Create your first application to start tracking companies, statuses, and notes."}
                            </div>
                            <div>
                              <button
                                onClick={openCreate}
                                className="mt-1 rounded-lg bg-slate-900 px-3 py-2 text-sm font-semibold text-white hover:bg-slate-800"
                              >
                                + Create your first application
                              </button>
                            </div>
                          </div>
                        </td>
                      </tr>
                    ) : (
                      items.map((row) => (
                        <tr key={row.id} className="border-t border-slate-100">
                          <td className="px-3 py-3">{row.company}</td>
                          <td className="px-3 py-3">{row.roleTitle}</td>
                          <td className="px-3 py-3">
                            <StatusBadge status={row.status} />
                          </td>
                          <td className="px-3 py-3 text-slate-600">{formatUtc(row.updatedAtUtc)}</td>
                          <td className="px-3 py-3">
                            <div className="flex gap-2">
                              <button
                                onClick={() => openEdit(row)}
                                disabled={loading}
                                className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                              >
                                Edit
                              </button>

                              <button
                                onClick={() => openEdit(row)}
                                disabled={loading}
                                title="Attachments"
                                aria-label={`Manage attachments for ${row.company} ${row.roleTitle}`}
                                className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-slate-200 bg-white hover:bg-slate-50 active:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                              >
                                📎
                              </button>

                              <button
                                onClick={() => openDeleteConfirm(row)}
                                disabled={loading}
                                className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                              >
                                Delete
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>

              {/* Pagination */}
              <div className="mt-3 flex items-center justify-between">
                <div className="text-sm text-slate-500">
                  Page {page} of {totalPages}
                </div>

                <div className="flex gap-2">
                  <button
                    onClick={() => setPage(1)}
                    disabled={loading || page <= 1}
                    className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium enabled:hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    First
                  </button>

                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={loading || page <= 1}
                    className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium enabled:hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Prev
                  </button>

                  <button
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={loading || page >= totalPages}
                    className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium enabled:hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Next
                  </button>

                  <button
                    onClick={() => setPage(totalPages)}
                    disabled={loading || page >= totalPages}
                    className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium enabled:hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Last
                  </button>
                </div>
              </div>
            </section>
          </>
        )}

        {/* Delete confirm modal */}
        {confirmDelete && (
          <div
            role="dialog"
            aria-modal="true"
            className="fixed inset-0 z-50 grid place-items-center bg-black/50 p-4"
            onMouseDown={(e) => {
              if (e.target === e.currentTarget) closeDeleteConfirm();
            }}
          >
            <div className="w-full max-w-md rounded-2xl bg-white p-4 shadow-xl">
              <h2 className="text-lg font-bold text-slate-900">Delete application?</h2>

              <p className="mt-2 text-sm text-slate-600">
                This will permanently delete{" "}
                <span className="font-semibold text-slate-900">
                  {confirmDelete.company} — {confirmDelete.roleTitle}
                </span>
                .
              </p>

              <div className="mt-4 flex justify-end gap-2">
                <button
                  onClick={closeDeleteConfirm}
                  disabled={deleting}
                  className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  Cancel
                </button>

                <button
                  onClick={async () => {
                    const row = confirmDelete;
                    setDeleting(true);
                    try {
                      closeDeleteConfirm();
                      await onDelete(row);
                    } finally {
                      setDeleting(false);
                    }
                  }}
                  disabled={deleting}
                  className="rounded-lg bg-red-600 px-3 py-2 text-sm font-semibold text-white hover:bg-red-700 active:bg-red-800 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {deleting ? "Deleting…" : "Delete"}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Create / Edit Application Modal */}
        {showModal && (
          <div
            role="dialog"
            aria-modal="true"
            className="fixed inset-0 z-50 grid place-items-center bg-black/50 p-4"
            onMouseDown={(e) => {
              if (saving) return;
              if (e.target === e.currentTarget) closeModal();
            }}
          >
            <div className="w-full max-w-3xl rounded-2xl bg-white p-4 shadow-xl">
              <h2 className="text-xl font-bold">{editing ? "Edit Application" : "New Application"}</h2>

              {editing && (
                <div className="mt-1">
                  <StatusBadge status={form.status} />
                </div>
              )}

              <div className="mt-4 grid gap-3">
                <label className="grid gap-1.5 text-sm font-medium text-slate-700">
                  Company
                  <input
                    value={form.company}
                    onChange={(e) => setForm((p) => ({ ...p, company: e.target.value }))}
                    className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm outline-none focus:border-slate-300 focus:ring-2 focus:ring-slate-200"
                  />
                </label>

                <label className="grid gap-1.5 text-sm font-medium text-slate-700">
                  Role Title
                  <input
                    value={form.roleTitle}
                    onChange={(e) => setForm((p) => ({ ...p, roleTitle: e.target.value }))}
                    className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm outline-none focus:border-slate-300 focus:ring-2 focus:ring-slate-200"
                  />
                </label>

                <label className="grid gap-1.5 text-sm font-medium text-slate-700">
                  Status
                  <select
                    value={form.status}
                    onChange={(e) => {
                      const v = e.target.value;
                      if (isApplicationStatus(v)) setForm((p) => ({ ...p, status: v }));
                    }}
                    className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm outline-none focus:border-slate-300 focus:ring-2 focus:ring-slate-200"
                  >
                    {STATUS_OPTIONS.map((s) => (
                      <option key={s} value={s}>
                        {s === "Draft" ? "Wishlist" : s}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="grid gap-1.5 text-sm font-medium text-slate-700">
                  Notes
                  <textarea
                    value={form.notes}
                    onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))}
                    rows={5}
                    className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm outline-none focus:border-slate-300 focus:ring-2 focus:ring-slate-200"
                  />
                </label>
              </div>

              {/* Attachments */}
              <div className="pt-2">
                {editing ? (
                  !apiBaseUrl || !token ? (
                    <div className="rounded-xl border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
                      Attachments are unavailable until the API base URL and token are available.
                    </div>
                  ) : (
                    <AttachmentsCard jobAppId={editing.id} apiBaseUrl={apiBaseUrl} token={token} />
                  )
                ) : (
                  <div className="rounded-xl border border-slate-200 bg-slate-50 p-3 text-sm text-slate-600">
                    Save the application first to enable attachments.
                  </div>
                )}
              </div>

              <div className="mt-4 flex justify-end gap-2">
                <button
                  onClick={closeModal}
                  disabled={saving}
                  className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium enabled:hover:bg-slate-50 disabled:opacity-50"
                >
                  Cancel
                </button>
                <button
                  onClick={onSave}
                  disabled={saving}
                  className="rounded-lg bg-slate-900 px-3 py-2 text-sm font-semibold text-white enabled:hover:bg-slate-800 disabled:opacity-50"
                >
                  {saving ? "Saving…" : "Save"}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
