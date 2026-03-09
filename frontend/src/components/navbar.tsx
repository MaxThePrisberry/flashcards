"use client";

import Link from "next/link";
import { Layers } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/components/auth-provider";
import { useState } from "react";
import AuthModal from "@/components/auth-modal";

export default function Navbar() {
  const { user, isLoading, logout } = useAuth();
  const [authOpen, setAuthOpen] = useState(false);
  const [authMode, setAuthMode] = useState<"login" | "register">("login");

  return (
    <nav className="h-16 border-b border-border bg-background flex items-center justify-between px-6">
      <Link href="/" className="flex items-center gap-2 text-lg font-semibold">
        <Layers className="h-5 w-5" />
        Flashcards
      </Link>

      <div className="flex items-center gap-2">
        {isLoading ? null : user ? (
          <>
            <Button variant="ghost" asChild>
              <Link href="/decks">My Decks</Link>
            </Button>
            <span className="text-sm text-muted-foreground">
              {user.displayName}
            </span>
            <Button variant="ghost" onClick={logout}>
              Logout
            </Button>
          </>
        ) : (
          <>
            <Button
              variant="ghost"
              onClick={() => {
                setAuthMode("login");
                setAuthOpen(true);
              }}
            >
              Login
            </Button>

            <Button
              variant="ghost"
              onClick={() => {
                setAuthMode("register");
                setAuthOpen(true);
              }}
            >
              Register
            </Button>
          </>
        )}
      </div>
      <AuthModal
        key={authMode}
        isOpen={authOpen}
        onClose={() => setAuthOpen(false)}
        initialMode={authMode}
      />
    </nav>
  );
}
