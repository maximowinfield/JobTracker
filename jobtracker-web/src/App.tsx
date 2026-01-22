import type React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthProvider";
import { useAuth } from "./context/useAuth";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import JobAppsPage from "./pages/JobAppsPage";
import { Toaster } from "react-hot-toast";

/**
 * Protected Route Wrapper
 * - Purpose: Prevent unauthenticated users from accessing protected routes (like /apps).
 * - Interview talking point: Auth state is derived from whether a token exists.
 */
function Protected({ children }: { children: React.ReactElement }) {
  const { token } = useAuth();

  // If no token, redirect to login and replace history so "back" doesn't re-open protected routes.
  if (!token) return <Navigate to="/login" replace />;

  return children;
}

/**
 * App Root
 * - Purpose: Defines global providers (AuthProvider), routing, and top-level layout.
 * - Interview talking point: AuthProvider wraps the app so any component can access auth state via context.
 */
export default function App() {
  return (
    <>
      {/* Global toast notifications (success/error/loading UX feedback) */}
      <Toaster
        position="top-right"
        toastOptions={{
          className: "text-sm",
          style: { borderRadius: "12px" },
        }}
      />

      {/* AuthProvider wraps routing so all routes/pages can access auth state */}
      <AuthProvider>
        <BrowserRouter>
          {/* App layout wrapper */}
          <div className="min-h-screen w-full bg-neutral-900">
            {/* Constrain + center the routed content */}
            <div className="mx-auto w-full max-w-7xl">
              <Routes>
                {/* Public routes */}
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />

                {/* Protected route: user must be authenticated */}
                <Route
                  path="/apps"
                  element={
                    <Protected>
                      <JobAppsPage />
                    </Protected>
                  }
                />

                {/* Catch-all: send unknown routes to the app's main page */}
                <Route path="*" element={<Navigate to="/apps" replace />} />
              </Routes>
            </div>
          </div>
        </BrowserRouter>
      </AuthProvider>
    </>
  );
}
