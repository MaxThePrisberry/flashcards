const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function login(email: string, password: string) {
  const res = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ email, password }),
  });

  if (!res.ok) {
    throw new Error("Login failed");
  }

  return res.json();
}

export async function signup(
  email: string,
  password: string,
  displayName: string,
) {
  const res = await fetch(`${API_URL}/api/auth/signup`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ email, password, displayName }),
  });

  if (!res.ok) {
    throw new Error("Signup failed");
  }

  return res.json();
}
