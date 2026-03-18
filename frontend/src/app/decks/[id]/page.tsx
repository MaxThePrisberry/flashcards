"use client";

import Link from "next/link";
import { use, useEffect, useMemo, useState } from "react";
import { ArrowRight, Brain, Layers3 } from "lucide-react";
import { getDeck } from "@/lib/api/decks";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { ApiError } from "@/lib/api/api-client";
import type { DeckDetailDto } from "@/lib/types";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

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
    return () => {
      controller.abort();
    };
  }, [id, isAuthenticated]);

  const orderedCards = useMemo(
    () => [...(deck?.cards ?? [])].sort((a, b) => a.position - b.position),
    [deck?.cards],
  );

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
    <main className="mx-auto flex w-full max-w-5xl flex-col gap-6 px-6 py-8">
      <Card className="border-border/70 bg-card/80">
        <CardHeader className="gap-4 md:flex-row md:items-start md:justify-between">
          <div className="space-y-3">
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Layers3 className="h-4 w-4" />
              <span>{orderedCards.length} cards</span>
            </div>

            <div className="space-y-2">
              <CardTitle className="text-3xl">{deck.title}</CardTitle>
              <CardDescription className="max-w-2xl text-base leading-7">
                {deck.description || "No description yet."}
              </CardDescription>
            </div>
          </div>

          <div className="flex shrink-0 items-center gap-3">
            <Button asChild variant="outline">
              <Link href={`/decks/${deck.id}/edit`}>Edit Deck</Link>
            </Button>

            <Button asChild size="lg" className="gap-2">
              <Link href={`/decks/${deck.id}/study`}>
                <Brain className="h-4 w-4" />
                Study Deck
              </Link>
            </Button>
          </div>
        </CardHeader>
      </Card>

      <section className="space-y-4">
        <div className="flex items-center justify-between gap-3">
          <div>
            <h2 className="text-xl font-semibold">Cards</h2>
            <p className="text-sm text-muted-foreground">
              Preview the deck or jump into study mode.
            </p>
          </div>

          <Button asChild variant="outline" className="gap-2">
            <Link href={`/decks/${deck.id}/study`}>
              Start studying
              <ArrowRight className="h-4 w-4" />
            </Link>
          </Button>
        </div>

        {orderedCards.length === 0 ? (
          <Card className="border-dashed">
            <CardContent className="py-10 text-center text-muted-foreground">
              This deck does not have any cards yet.
            </CardContent>
          </Card>
        ) : (
          <div className="grid gap-4 md:grid-cols-2">
            {orderedCards.map((card, index) => (
              <Card
                key={card.id}
                className="border-border/70 bg-card/70 transition-colors hover:bg-accent/40"
              >
                <CardHeader className="gap-3">
                  <div className="text-xs uppercase tracking-wide text-muted-foreground">
                    Card {index + 1}
                  </div>
                  <CardTitle className="text-lg">{card.term}</CardTitle>
                  <CardDescription className="line-clamp-3 text-sm leading-6 text-foreground/80">
                    {card.definition}
                  </CardDescription>
                </CardHeader>
              </Card>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
