import axios from "axios";

/**
 * API Client Setup
 * - Purpose: Centralize HTTP configuration (base URL, JSON headers, auth header injection).
 * - Interview talking point: One client ensures consistent behavior across all API calls.
 */
const baseURL = import.meta.env.VITE_API_URL ?? "http://localhost:5137";

export const api = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

/**
 * setAuthToken
 * - Purpose: Attach or remove the Authorization header for all future API requests.
 * - Interview talking point: Frontend auth is enforced by including `Bearer <token>` on requests.
 */
export function setAuthToken(token: string | null) {
  if (token) {
    api.defaults.headers.common.Authorization = `Bearer ${token}`;
  } else {
    delete api.defaults.headers.common.Authorization;
  }
}

/**
 * IMPORTANT: Attach token immediately on first load
 * - Purpose: If the user refreshes the page, we restore auth headers before any requests are made.
 * - Interview talking point: Prevents "flash of unauthenticated" request failures after refresh.
 */
const existingToken = localStorage.getItem("jt_token");
if (existingToken) {
  setAuthToken(existingToken);
}
