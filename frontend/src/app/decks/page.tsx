"use client";

import { useEffect, useState } from "react";
import { getDecks } from "../lib/api/decks";
import DeckCard from "../components/deck-card";
import Link from "next/link";
import { Button } from "@/components/ui/button";

export default function DecksPage() {
  const [decks, setDecks] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const data = await getDecks();
        setDecks(data.items);
      } finally {
        setLoading(false);
      }
    }

    load();
  }, []);

  if (loading) {
    return <main className="p-10">Loading decks...</main>;
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
          <DeckCard key={deck.id} {...deck} />
        ))}
      </div>
    </main>
  );
}
