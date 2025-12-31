import type React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthProvider";
import { useAuth } from "./context/useAuth";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import JobAppsPage from "./pages/JobAppsPage";
import { Toaster } from "react-hot-toast";

function Protected({ children }: { children: React.ReactElement }) {
  const { token } = useAuth();
  if (!token) return <Navigate to="/login" replace />;
  return children;
}

export default function App() {
  return (
    <>
      <Toaster
        position="top-right"
        toastOptions={{
          className: "text-sm",
          style: { borderRadius: "12px" },
        }}
      />

<AuthProvider>
  <BrowserRouter>
    {/* App layout wrapper */}
    <div className="min-h-screen w-full bg-neutral-900">
      {/* ✅ Constrain + center the routed content */}
      <div className="mx-auto w-full max-w-7xl">
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          <Route
            path="/apps"
            element={
              <Protected>
                <JobAppsPage />
              </Protected>
            }
          />

          <Route path="*" element={<Navigate to="/apps" replace />} />
        </Routes>
      </div>
    </div>
  </BrowserRouter>
</AuthProvider>

    </>
  );
}
