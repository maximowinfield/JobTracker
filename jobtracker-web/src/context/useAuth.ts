import { useContext } from "react";
import { AuthContext } from "./AuthContext";

/**
 * useAuth (Custom Hook)
 * - Purpose: Standard, safe way for components to access auth state/actions.
 * - Interview talking point: Centralizes access and fails fast if provider wiring is missing.
 */
export function useAuth() {
  const ctx = useContext(AuthContext);

  // Fail fast if someone uses the hook outside <AuthProvider>.
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");

  return ctx;
}
