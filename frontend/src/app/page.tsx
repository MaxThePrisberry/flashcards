"use client";

import Link from "next/link";
import { Layers } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/components/auth-provider";
import { useAuthModal } from "@/components/auth-modal-provider";

export default function Home() {
  const { user, isLoading } = useAuth();
  const { openAuthModal } = useAuthModal();

  return (
    <main className="flex flex-col items-center justify-center flex-1 gap-6 text-center px-4">
      <Layers className="h-16 w-16 text-primary" />
      <h1 className="text-4xl font-bold tracking-tight">Flashcards</h1>
      <p className="text-lg text-muted-foreground">
        Create decks. Study smarter. Track progress.
      </p>

      {!isLoading && (
        <div className="flex gap-4">
          {user ? (
            <>
              <Button asChild size="lg">
                <Link href="/decks">My Decks</Link>
              </Button>
              <Button asChild variant="outline" size="lg">
                <Link href="/decks/create">Create Deck</Link>
              </Button>
            </>
          ) : (
            <>
              <Button size="lg" onClick={() => openAuthModal("login")}>
                Login
              </Button>
              <Button variant="outline" size="lg" onClick={() => openAuthModal("register")}>
                Register
              </Button>
            </>
          )}
        </div>
      )}
    </main>
  );
}
