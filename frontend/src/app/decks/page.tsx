"use client";

import { useEffect, useState } from "react";
import { getDecks } from "@/lib/api/decks";
import DeckCard from "@/components/deck-card";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import type { DeckDto } from "@/lib/types";

export default function DecksPage() {
  const [decks, setDecks] = useState<DeckDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const data = await getDecks();
        if (!cancelled) setDecks(data.items);
      } catch {
        if (!cancelled) setError("Failed to load decks");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => { cancelled = true; };
  }, []);

  if (loading) {
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
    </main>
  );
}
