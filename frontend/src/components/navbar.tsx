"use client";

import Link from "next/link";
import { Layers } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/components/auth-provider";

export default function Navbar() {
  const { user, isLoading, logout } = useAuth();

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
            <Button variant="ghost" asChild>
              <Link href="/decks/create">Create Deck</Link>
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
            <Button variant="ghost" asChild>
              <Link href="/login">Login</Link>
            </Button>
            <Button variant="ghost" asChild>
              <Link href="/register">Register</Link>
            </Button>
          </>
        )}
      </div>
    </nav>
  );
}
