import { api } from "./client";

/**
 * ApplicationStatus
 * - IMPORTANT: These must match backend enum names exactly.
 * - Interview talking point: Shared enum values keep frontend columns and backend status consistent.
 */
export type ApplicationStatus =
  | "Draft"
  | "Applied"
  | "Interviewing"
  | "Offer"
  | "Rejected"
  | "Accepted";

/**
 * JobAppDto
 * - Purpose: Frontend shape of a Job Application record returned from the API.
 * - Interview talking point: DTOs prevent exposing EF entities directly and keep contracts stable.
 */
export type JobAppDto = {
  id: number;
  company: string;
  roleTitle: string;
  status: ApplicationStatus;
  notes: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
};

/**
 * PagedResult<T>
 * - Purpose: Standard paging contract so UI can handle large datasets efficiently.
 */
export type PagedResult<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
};

const JOB_APPS_PATH = "/job-apps";

/**
 * listJobApps
 * - Purpose: Fetch job apps with optional filters + pagination.
 * - Interview talking point: Keeps API calls out of UI components (separation of concerns).
 */
export async function listJobApps(params: {
  q?: string;
  status?: ApplicationStatus;
  page?: number;
  pageSize?: number;
}) {
  const res = await api.get<PagedResult<JobAppDto>>(JOB_APPS_PATH, { params });
  return res.data;
}

/**
 * createJobApp
 * - Purpose: Create a new job application record.
 */
export async function createJobApp(payload: {
  company: string;
  roleTitle: string;
  status?: ApplicationStatus;
  notes?: string | null;
}) {
  const res = await api.post<JobAppDto>(JOB_APPS_PATH, payload);
  return res.data;
}

/**
 * updateJobApp
 * - Purpose: Partial updates (PATCH) so the UI can update only what changed.
 * - Interview talking point: PATCH is ideal for status moves in a Kanban UI.
 */
export async function updateJobApp(
  id: number,
  payload: Partial<{
    company: string;
    roleTitle: string;
    status: ApplicationStatus;
    notes: string | null;
  }>
) {
  const res = await api.patch<JobAppDto>(`${JOB_APPS_PATH}/${id}`, payload);
  return res.data;
}

/**
 * deleteJobApp
 * - Purpose: Delete an application record.
 */
export async function deleteJobApp(id: number) {
  await api.delete(`${JOB_APPS_PATH}/${id}`);
}
