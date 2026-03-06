"use client";

import { useEffect, useState } from "react";
import { getDecks } from "@/lib/api/decks";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { ApiError } from "@/lib/api/api-client";
import DeckCard from "@/components/deck-card";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Layers } from "lucide-react";
import type { DeckSummaryDto } from "@/lib/types";

export default function DecksPage() {
  const { isAuthenticated, isLoading: authLoading } = useRequireAuth();
  const [decks, setDecks] = useState<DeckSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;

    let cancelled = false;

    async function load() {
      try {
        const data = await getDecks();
        if (!cancelled) setDecks(data.items);
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : "Failed to load decks");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => { cancelled = true; };
  }, [isAuthenticated]);

  if (authLoading || loading) {
    return <main className="p-10">Loading decks...</main>;
  }

  if (error) {
    return <main className="p-10 text-destructive">{error}</main>;
  }

  return (
    <main className="max-w-4xl mx-auto p-6 space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-semibold">Your Decks</h1>

        <Button asChild>
          <Link href="/decks/create">Create Deck</Link>
        </Button>
      </div>

      {decks.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16 text-center gap-4">
          <Layers className="h-12 w-12 text-muted-foreground" />
          <p className="text-lg text-muted-foreground">No decks yet</p>
          <Button asChild>
            <Link href="/decks/create">Create your first deck</Link>
          </Button>
        </div>
      ) : (
        <div className="grid gap-4">
          {decks.map((deck) => (
            <DeckCard
              key={deck.id}
              id={deck.id}
              title={deck.title}
              description={deck.description}
              cardCount={deck.cardCount}
            />
          ))}
        </div>
      )}
    </main>
  );
}
