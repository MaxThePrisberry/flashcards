"use client";

import { use, useEffect, useState } from "react";
import { getDeck } from "@/lib/api/decks";
import type { DeckDto } from "@/lib/types";

export default function DeckDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const [deck, setDeck] = useState<DeckDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const data = await getDeck(id);
        if (!cancelled) setDeck(data);
      } catch {
        if (!cancelled) setError("Failed to load deck");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => { cancelled = true; };
  }, [id]);

  if (loading) {
    return <main className="p-10">Loading deck...</main>;
  }

  if (error) {
    return <main className="p-10 text-destructive">{error}</main>;
  }

  if (!deck) {
    return <main className="p-10">Deck not found</main>;
  }

  return (
    <main className="max-w-3xl mx-auto p-6 space-y-6">
      <h1 className="text-2xl font-semibold">{deck.title}</h1>

      {deck.description && (
        <p className="text-muted-foreground">{deck.description}</p>
      )}

      <div className="space-y-3">
        <h2 className="text-lg font-medium">Cards</h2>

        {deck.cards?.map((card) => (
          <div key={card.id} className="border rounded-lg p-4 bg-card">
            <div>
              <strong>Front:</strong> {card.term}
            </div>
            <div>
              <strong>Back:</strong> {card.definition}
            </div>
          </div>
        ))}
      </div>
    </main>
  );
}
