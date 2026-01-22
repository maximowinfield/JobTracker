import { createContext } from "react";

/**
 * AuthContextValue (Context Contract)
 * - Purpose: Defines what auth state and actions are available to the rest of the app.
 * - Interview talking point: Context exposes state (token) plus controlled actions (setToken/logout).
 */
export type AuthContextValue = {
  // Authentication token (JWT). null means logged out.
  token: string | null;

  // Controlled way to update token (login sets token, logout clears token).
  setToken: (token: string | null) => void;

  // Convenience action that performs logout cleanup in one place.
  logout: () => void;
};

/**
 * AuthContext
 * - Default value is null so we can detect misuse (calling useAuth outside AuthProvider).
 * - Interview talking point: Fail-fast helps catch wiring mistakes early.
 */
export const AuthContext = createContext<AuthContextValue | null>(null);
