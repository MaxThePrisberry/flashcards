"use client";

import { use, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";
import { getDeck, updateDeck } from "@/lib/api/decks";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { ApiError } from "@/lib/api/api-client";
import DeckForm from "@/components/deck-form";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import type { DeckDetailDto } from "@/lib/types";

export default function EditDeckPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading } = useRequireAuth();

  const [deck, setDeck] = useState<DeckDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;

    const controller = new AbortController();

    async function loadDeck() {
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

    loadDeck();

    return () => controller.abort();
  }, [id, isAuthenticated]);

  async function handleSubmit(data: {
    title: string;
    description: string;
    cards: { term: string; definition: string; termType: string; definitionType: string }[];
  }) {
    try {
      await updateDeck(id, data);
      router.push(`/decks/${id}`);
    } catch (err) {
      if (err instanceof ApiError) {
        alert(err.message);
      } else {
        alert("Failed to update deck");
      }
    }
  }

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
      <Button variant="ghost" asChild className="gap-2">
        <Link href={`/decks/${deck.id}`}>
          <ArrowLeft className="h-4 w-4" />
          Back to Deck
        </Link>
      </Button>

      <DeckForm
        title="Edit Deck"
        submitLabel="Save Changes"
        submittingLabel="Saving..."
        initialData={{
          title: deck.title,
          description: deck.description,
          cards: deck.cards.map((c) => ({
            term: c.term,
            definition: c.definition,
            termType: c.termType,
            definitionType: c.definitionType,
          })),
        }}
        onSubmit={handleSubmit}
      />
    </main>
  );
}
