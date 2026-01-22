import React, { useCallback, useEffect, useMemo, useState } from "react";
import { setAuthToken } from "../api/client";
import { AuthContext } from "./AuthContext";

/**
 * AuthProvider
 * - Purpose: Centralizes authentication state and keeps it in sync with:
 *   1) React state (in-memory)
 *   2) localStorage (persistence across refresh)
 *   3) API client (Authorization header injection)
 * - Interview talking point: This is the "single source of truth" for auth in the frontend.
 */
export function AuthProvider({ children }: { children: React.ReactNode }) {
  /**
   * Auth state initialization (lazy initializer)
   * - Reads token from localStorage once on first render.
   * - Interview talking point: Using a function avoids reading localStorage on every re-render.
   */
  const [token, setTokenState] = useState<string | null>(() =>
    localStorage.getItem("jt_token")
  );

  /**
   * setToken (single entry point for auth changes)
   * - Updates state, persists/removes token in localStorage, and updates API Authorization header.
   * - Interview talking point: Prevents auth from getting out of sync between UI and network layer.
   */
  const setToken = (t: string | null) => {
    setTokenState(t);

    // Persist across refresh if token exists; otherwise remove it.
    if (t) localStorage.setItem("jt_token", t);
    else localStorage.removeItem("jt_token");

    // Ensure API client attaches/removes Bearer token immediately.
    setAuthToken(t);
  };

  /**
   * logout action
   * - Clears token using the central setToken function.
   * - useCallback keeps function reference stable for context consumers.
   */
  const logout = useCallback(() => setToken(null), []);

  /**
   * Safety sync: ensure API client stays updated when token changes.
   * Note: setToken already calls setAuthToken, so this is mostly a safety net.
   */
  useEffect(() => {
    setAuthToken(token);
  }, [token]);

  /**
   * Memoize context value to avoid unnecessary re-renders in consumers.
   * Only changes when token/logout changes.
   */
  const value = useMemo(() => ({ token, setToken, logout }), [token, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
