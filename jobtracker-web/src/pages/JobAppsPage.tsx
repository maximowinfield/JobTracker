import { useEffect } from "react";
import { api } from "../api/client";

export default function JobAppsPage() {
  useEffect(() => {
    api.get("/api/me").then(res => {
      console.log("ME:", res.data);
    });
  }, []);

  return (
    <div style={{ padding: 24 }}>
      Job Apps UI coming next ✅
    </div>
  );
}
