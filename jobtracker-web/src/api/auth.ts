import { api } from "./client";

export type AuthResponse = {
  token: string;
};

export async function login(email: string, password: string) {
  const res = await api.post<AuthResponse>("/api/auth/login", {
    email,
    password,
  });
  return res.data;
}

export async function register(email: string, password: string) {
  const res = await api.post<AuthResponse>("/api/auth/register", {
    email,
    password,
  });
  return res.data;
}
