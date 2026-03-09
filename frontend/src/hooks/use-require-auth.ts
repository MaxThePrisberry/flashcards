import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth-provider";
import { useAuthModal } from "@/components/auth-modal-provider";

export function useRequireAuth() {
  const { user, isLoading } = useAuth();
  const router = useRouter();
  const { openAuthModal } = useAuthModal();

  useEffect(() => {
    if (!isLoading && !user) {
      router.replace("/");
      openAuthModal("login");
    }
  }, [user, isLoading, router, openAuthModal]);

  return { isAuthenticated: !!user, isLoading };
}
