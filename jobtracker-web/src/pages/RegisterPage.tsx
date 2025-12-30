import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../api/auth";
import { useAuth } from "../context/AuthContext";

function getStatus(err: unknown): number | undefined {
  if (typeof err === "object" && err !== null && "response" in err) {
    const resp = (err as { response?: { status?: number } }).response;
    return resp?.status;
  }
  return undefined;
}


export default function RegisterPage() {
  const { setToken } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const res = await register(email, password);
      setToken(res.token);                 // ✅ auto-login after register
      navigate("/apps", { replace: true }); // ✅ go to apps
    } catch (err: unknown) {
        const status = getStatus(err);


      // your backend returns:
      // 400 for missing/weak password
      // 409 if email exists
      if (status === 409) setError("That email is already registered. Try signing in.");
      else if (status === 400) setError("Password must be at least 8 characters (and email is required).");
      else setError("Registration failed. Try again.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ maxWidth: 420, margin: "64px auto", padding: 16 }}>
      <h1 style={{ marginBottom: 8 }}>Create account</h1>
      <p style={{ marginTop: 0, opacity: 0.8 }}>
        Register to start tracking your job applications.
      </p>

      <form onSubmit={onSubmit} style={{ display: "grid", gap: 12, marginTop: 16 }}>
        <label style={{ display: "grid", gap: 6 }}>
          Email
          <input
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
            placeholder="you@example.com"
            required
            style={{ padding: 10 }}
          />
        </label>

        <label style={{ display: "grid", gap: 6 }}>
          Password
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="new-password"
            type="password"
            placeholder="At least 8 characters"
            required
            style={{ padding: 10 }}
          />
        </label>

        {error && <div style={{ color: "crimson" }}>{error}</div>}

        <button disabled={loading} style={{ padding: 10, cursor: "pointer" }}>
          {loading ? "Creating..." : "Create account"}
        </button>
      </form>

      <div style={{ marginTop: 14, opacity: 0.85 }}>
        Already have an account? <Link to="/login">Sign in</Link>
      </div>
    </div>
  );
}
