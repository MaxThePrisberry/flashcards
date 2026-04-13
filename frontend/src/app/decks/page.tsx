"use client";

import Link from "next/link";
import { FormEvent, useCallback, useEffect, useRef, useState } from "react";
import { ChevronLeft, ChevronRight, Layers, Search } from "lucide-react";
import DeckCard from "@/components/deck-card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { ApiError } from "@/lib/api/api-client";
import { getDecks, getLikeStatus, likeDeck, unlikeDeck } from "@/lib/api/decks";
import type { DeckSummaryDto } from "@/lib/types";

export default function DecksPage() {
  const { isAuthenticated, isLoading: authLoading } = useRequireAuth();

  const [decks, setDecks] = useState<DeckSummaryDto[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [activeSearch, setActiveSearch] = useState("");
  const [likedStatuses, setLikedStatuses] = useState<Record<string, boolean>>(
    {},
  );
  const [pendingLikeIds, setPendingLikeIds] = useState<Record<string, boolean>>(
    {},
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const abortRef = useRef<AbortController | null>(null);

  const loadDecks = useCallback(
    async (targetPage: number, searchTerm: string) => {
      abortRef.current?.abort();
      const controller = new AbortController();
      abortRef.current = controller;

      setLoading(true);
      setError(null);

      try {
        const data = await getDecks(
          targetPage,
          20,
          searchTerm,
          controller.signal,
        );
        setDecks(data.items);
        setPage(data.page);
        setTotalPages(data.totalPages);

        if (searchTerm.trim()) {
          const statuses = await Promise.all(
            data.items.map(async (deck) => {
              try {
                const isLiked = await getLikeStatus(deck.id, controller.signal);
                return { id: deck.id, isLiked };
              } catch (err) {
                if (err instanceof DOMException && err.name === "AbortError") {
                  throw err;
                }

                return { id: deck.id, isLiked: false };
              }
            }),
          );

          if (!controller.signal.aborted) {
            const nextStatuses: Record<string, boolean> = {};
            for (const status of statuses) {
              nextStatuses[status.id] = status.isLiked;
            }
            setLikedStatuses(nextStatuses);
          }
        } else {
          setLikedStatuses({});
        }
      } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") return;
        setError(
          err instanceof ApiError ? err.message : "Failed to load decks",
        );
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    },
    [],
  );

  useEffect(() => {
    if (!isAuthenticated) return;

    void loadDecks(1, "");

    return () => {
      abortRef.current?.abort();
    };
  }, [isAuthenticated, loadDecks]);

  async function handleSearchSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const nextSearch = searchInput.trim();
    setActiveSearch(nextSearch);
    await loadDecks(1, nextSearch);
  }

  async function handleClearSearch() {
    setSearchInput("");
    setActiveSearch("");
    await loadDecks(1, "");
  }

  async function handleToggleLike(deckId: string) {
    if (pendingLikeIds[deckId]) return;

    const currentlyLiked = likedStatuses[deckId] ?? false;

    setPendingLikeIds((prev) => ({ ...prev, [deckId]: true }));
    setLikedStatuses((prev) => ({ ...prev, [deckId]: !currentlyLiked }));
    setDecks((prev) =>
      prev.map((deck) =>
        deck.id === deckId
          ? {
              ...deck,
              likeCount: Math.max(
                deck.likeCount + (currentlyLiked ? -1 : 1),
                0,
              ),
            }
          : deck,
      ),
    );

    try {
      if (currentlyLiked) {
        await unlikeDeck(deckId);
      } else {
        await likeDeck(deckId);
      }
    } catch (err) {
      setLikedStatuses((prev) => ({ ...prev, [deckId]: currentlyLiked }));
      setDecks((prev) =>
        prev.map((deck) =>
          deck.id === deckId
            ? {
                ...deck,
                likeCount: Math.max(
                  deck.likeCount + (currentlyLiked ? 1 : -1),
                  0,
                ),
              }
            : deck,
        ),
      );
      setError(err instanceof ApiError ? err.message : "Failed to update like");
    } finally {
      setPendingLikeIds((prev) => {
        const next = { ...prev };
        delete next[deckId];
        return next;
      });
    }
  }

  const isSearchMode = activeSearch.trim().length > 0;

  if (authLoading || loading) {
    return <main className="p-10">Loading decks...</main>;
  }

  if (error) {
    return <main className="p-10 text-destructive">{error}</main>;
  }

  return (
    <main className="mx-auto max-w-4xl space-y-6 p-6">
      <div className="flex items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold">My Decks</h1>
        <Button asChild>
          <Link href="/decks/create">Create Deck</Link>
        </Button>
      </div>

      <section className="space-y-3 rounded-lg border border-border/70 bg-card/60 p-4">
        <div className="space-y-1">
          <h2 className="text-lg font-medium">Search public decks</h2>
          <p className="text-sm text-muted-foreground">
            Search by deck title. Matching results include public decks and any
            of your own matching decks.
          </p>
        </div>

        <form
          onSubmit={handleSearchSubmit}
          className="flex flex-col gap-3 sm:flex-row"
        >
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              placeholder="Search deck titles"
              className="pl-9"
            />
          </div>

          <Button type="submit">Search</Button>

          {isSearchMode ? (
            <Button type="button" variant="outline" onClick={handleClearSearch}>
              Clear
            </Button>
          ) : null}
        </form>

        <p className="text-sm text-muted-foreground">
          {isSearchMode
            ? `Showing results for "${activeSearch}".`
            : "Not searching right now. This view shows your decks."}
        </p>
      </section>

      {decks.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-4 py-16 text-center">
          <Layers className="h-12 w-12 text-muted-foreground" />
          <p className="text-lg text-muted-foreground">
            {isSearchMode
              ? `No decks found for "${activeSearch}"`
              : "No decks yet"}
          </p>

          {isSearchMode ? (
            <Button variant="outline" onClick={handleClearSearch}>
              Clear search
            </Button>
          ) : (
            <Button asChild>
              <Link href="/decks/create">Create your first deck</Link>
            </Button>
          )}
        </div>
      ) : (
        <>
          <div className="grid gap-4">
            {decks.map((deck) => (
              <DeckCard
                key={deck.id}
                id={deck.id}
                title={deck.title}
                description={deck.description}
                cardCount={deck.cardCount}
                likeCount={deck.likeCount}
                showLikeButton={isSearchMode}
                isLiked={likedStatuses[deck.id] ?? false}
                liking={pendingLikeIds[deck.id] ?? false}
                onToggleLike={handleToggleLike}
              />
            ))}
          </div>

          {totalPages > 1 ? (
            <div className="flex items-center justify-center gap-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => void loadDecks(page - 1, activeSearch)}
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
                onClick={() => void loadDecks(page + 1, activeSearch)}
                disabled={page >= totalPages}
              >
                Next
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          ) : null}
        </>
      )}
    </main>
  );
}
