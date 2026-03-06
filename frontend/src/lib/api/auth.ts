import type { AuthResponse } from "@/lib/types";
import { ApiError } from "@/lib/api/api-client";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function login(
  email: string,
  password: string,
): Promise<AuthResponse> {
  const res = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => null);
    throw new ApiError(
      res.status,
      body?.error ?? "unknown_error",
      body?.message ?? "Login failed",
      body?.details,
    );
  }

  return res.json();
}

export async function signup(
  email: string,
  password: string,
  displayName: string,
): Promise<AuthResponse> {
  const res = await fetch(`${API_URL}/api/auth/signup`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password, displayName }),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => null);
    throw new ApiError(
      res.status,
      body?.error ?? "unknown_error",
      body?.message ?? "Signup failed",
      body?.details,
    );
  }

  return res.json();
}
