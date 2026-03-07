"use client";

import { useEffect, useState, useCallback, useRef } from "react";
import { getDecks } from "@/lib/api/decks";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { ApiError } from "@/lib/api/api-client";
import DeckCard from "@/components/deck-card";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Layers, ChevronLeft, ChevronRight } from "lucide-react";
import type { DeckSummaryDto } from "@/lib/types";

export default function DecksPage() {
  const { isAuthenticated, isLoading: authLoading } = useRequireAuth();
  const [decks, setDecks] = useState<DeckSummaryDto[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const loadDecks = useCallback(async (targetPage: number) => {
    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setLoading(true);
    setError(null);
    try {
      const data = await getDecks(targetPage, 20, controller.signal);
      setDecks(data.items);
      setPage(data.page);
      setTotalPages(data.totalPages);
    } catch (err) {
      if (err instanceof DOMException && err.name === "AbortError") return;
      setError(err instanceof ApiError ? err.message : "Failed to load decks");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!isAuthenticated) return;
    loadDecks(1);
  }, [isAuthenticated, loadDecks]);

  if (authLoading || loading) {
    return <main className="p-10">Loading decks...</main>;
  }

  if (error) {
    return <main className="p-10 text-destructive">{error}</main>;
  }

  return (
    <main className="max-w-4xl mx-auto p-6 space-y-6">
      {decks.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16 text-center gap-4">
          <h1 className="text-2xl font-semibold">Your Decks</h1>
          <Layers className="h-12 w-12 text-muted-foreground" />
          <p className="text-lg text-muted-foreground">No decks yet</p>
          <Button asChild>
            <Link href="/decks/create">Create your first deck</Link>
          </Button>
        </div>
      ) : (
        <>
          <div className="flex justify-between items-center gap-4">
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

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => loadDecks(page - 1)}
                disabled={page <= 1}
              >
                <ChevronLeft className="h-4 w-4" />
                Previous
              </Button>
              <span className="text-sm text-muted-foreground">
                Page {page} of {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => loadDecks(page + 1)}
                disabled={page >= totalPages}
              >
                Next
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          )}
        </>
      )}
    </main>
  );
}
