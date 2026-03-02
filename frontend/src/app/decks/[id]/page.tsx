"use client";

import { useEffect, useState } from "react";
import { getDeck } from "@/app/lib/api/decks";

export default function DeckDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const [deck, setDeck] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      const { id } = await params;

      try {
        const data = await getDeck(id);
        setDeck(data);
      } finally {
        setLoading(false);
      }
    }

    load();
  }, [params]);

  if (loading) {
    return <main className="p-10">Loading deck...</main>;
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

        {deck.cards?.map((card: any) => (
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
