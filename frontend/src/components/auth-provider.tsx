"use client";

import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  useMemo,
} from "react";
import { useRouter } from "next/navigation";
import { login as apiLogin, signup as apiSignup } from "@/lib/api/auth";
import type { UserDto, AuthResponse } from "@/lib/types";

interface AuthContextValue {
  user: UserDto | null;
  token: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  signup: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}

export default function AuthProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [user, setUser] = useState<UserDto | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedToken = localStorage.getItem("token");
    const storedUser = localStorage.getItem("user");

    if (storedToken && storedUser) {
      try {
        setToken(storedToken);
        setUser(JSON.parse(storedUser));
      } catch {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
      }
    }
    setIsLoading(false);
  }, []);

  const handleAuthResponse = useCallback((data: AuthResponse) => {
    setToken(data.token);
    setUser(data.user);
    localStorage.setItem("token", data.token);
    localStorage.setItem("user", JSON.stringify(data.user));
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      const data = await apiLogin(email, password);
      handleAuthResponse(data);
    },
    [handleAuthResponse],
  );

  const signup = useCallback(
    async (email: string, password: string, displayName: string) => {
      const data = await apiSignup(email, password, displayName);
      handleAuthResponse(data);
    },
    [handleAuthResponse],
  );

  const logout = useCallback(() => {
    setToken(null);
    setUser(null);
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    router.push("/");
  }, [router]);

  const value = useMemo(
    () => ({ user, token, isLoading, login, signup, logout }),
    [user, token, isLoading, login, signup, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
