import { api } from "./client";

/**
 * IMPORTANT:
 * These must match your backend enum names EXACTLY (case-sensitive).
 * If you’re unsure, open: api/JobTracker.Api/Models/ApplicationStatus.cs
 */
export type ApplicationStatus =
  | "Draft"
  | "Applied"
  | "Interviewing"
  | "Offer"
  | "Rejected"
  | "Accepted";

export type JobAppDto = {
  id: number;
  company: string;
  roleTitle: string;
  status: ApplicationStatus;
  notes: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type PagedResult<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
};

const JOB_APPS_PATH = "/job-apps";

export async function listJobApps(params: {
  q?: string;
  status?: ApplicationStatus;
  page?: number;
  pageSize?: number;
}) {
  const res = await api.get<PagedResult<JobAppDto>>(JOB_APPS_PATH, { params });
  return res.data;
}

export async function createJobApp(payload: {
  company: string;
  roleTitle: string;
  status?: ApplicationStatus;
  notes?: string | null;
}) {
  const res = await api.post<JobAppDto>(JOB_APPS_PATH, payload);
  return res.data;
}

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

export async function deleteJobApp(id: number) {
  await api.delete(`${JOB_APPS_PATH}/${id}`);
}
