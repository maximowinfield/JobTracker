import React, { useCallback, useEffect, useMemo, useState } from "react";
import { setAuthToken } from "../api/client";
import { AuthContext } from "./AuthContext";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setTokenState] = useState<string | null>(() =>
    localStorage.getItem("jt_token")
  );

  const setToken = (t: string | null) => {
    setTokenState(t);
    if (t) localStorage.setItem("jt_token", t);
    else localStorage.removeItem("jt_token");
    setAuthToken(t);
  };

  const logout = useCallback(() => setToken(null), []);

  useEffect(() => {
    setAuthToken(token);
  }, [token]);

  const value = useMemo(() => ({ token, setToken, logout }), [token, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
