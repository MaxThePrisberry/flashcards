"use client";

import { use, useEffect, useState } from "react";
import { getDeck } from "@/lib/api/decks";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { ApiError } from "@/lib/api/api-client";
import type { DeckDetailDto } from "@/lib/types";

export default function DeckDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { isAuthenticated, isLoading: authLoading } = useRequireAuth();
  const [deck, setDeck] = useState<DeckDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;

    const controller = new AbortController();

    async function load() {
      try {
        const data = await getDeck(id, controller.signal);
        setDeck(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") return;
        setError(err instanceof ApiError ? err.message : "Failed to load deck");
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }

    load();
    return () => { controller.abort(); };
  }, [id, isAuthenticated]);

  if (authLoading || loading) {
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

        {deck.cards.map((card) => (
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
