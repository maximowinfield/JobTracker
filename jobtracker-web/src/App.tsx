import type React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "./context/AuthContext";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import JobAppsPage from "./pages/JobAppsPage";

function Protected({ children }: { children: React.ReactElement }) {
  const { token } = useAuth();
  if (!token) return <Navigate to="/login" replace />;
  return children;
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
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
      </BrowserRouter>
    </AuthProvider>
  );
}
