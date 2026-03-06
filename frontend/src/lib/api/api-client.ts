import type { ErrorResponse } from "@/lib/types";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export class ApiError extends Error {
  status: number;
  errorCode: string;
  details?: Record<string, string[]>;

  constructor(
    status: number,
    errorCode: string,
    message: string,
    details?: Record<string, string[]>,
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errorCode = errorCode;
    this.details = details;
  }
}

export async function apiFetch(
  path: string,
  options: RequestInit = {},
): Promise<Response> {
  const token = localStorage.getItem("token");

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...((options.headers as Record<string, string>) || {}),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    if (res.status === 401) {
      window.dispatchEvent(new CustomEvent("auth:unauthorized"));
    }

    await throwApiError(res, `Request failed with status ${res.status}`);
  }

  return res;
}

export async function throwApiError(
  res: Response,
  fallbackMessage: string,
): Promise<never> {
  let body: ErrorResponse | null = null;
  try {
    body = await res.json();
  } catch {
    // response may not have a JSON body
  }

  throw new ApiError(
    res.status,
    body?.error ?? "unknown_error",
    body?.message ?? fallbackMessage,
    body?.details,
  );
}
