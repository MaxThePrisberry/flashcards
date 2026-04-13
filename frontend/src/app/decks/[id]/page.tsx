"use client";

import Link from "next/link";
import { use, useEffect, useMemo, useState } from "react";
import { ArrowRight, Brain, ClipboardCheck, Clock3, Layers3, RefreshCcw } from "lucide-react";
import { getDeck, getLatestReview } from "@/lib/api/decks";
import { ApiError } from "@/lib/api/api-client";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { useAuth } from "@/components/auth-provider";
import type { DeckDetailDto, ReviewSessionDto } from "@/lib/types";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

function formatReviewedAt(value: string): string {
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export default function DeckDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { isAuthenticated, isLoading: authLoading } = useRequireAuth();
  const { user } = useAuth();

  const [deck, setDeck] = useState<DeckDetailDto | null>(null);
  const [latestReview, setLatestReview] = useState<ReviewSessionDto | null>(
    null,
  );
  const [historyUnavailable, setHistoryUnavailable] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;

    const controller = new AbortController();

    async function load() {
      try {
        const deckData = await getDeck(id, controller.signal);
        setDeck(deckData);

        try {
          const reviewData = await getLatestReview(id, controller.signal);
          setLatestReview(reviewData);
          setHistoryUnavailable(false);
        } catch (reviewError) {
          if (
            reviewError instanceof DOMException &&
            reviewError.name === "AbortError"
          ) {
            return;
          }

          if (
            reviewError instanceof ApiError &&
            reviewError.status === 404
          ) {
            setLatestReview(null);
            setHistoryUnavailable(false);
          } else {
            setLatestReview(null);
            setHistoryUnavailable(true);
          }
        }
      } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") return;
        setError(err instanceof ApiError ? err.message : "Failed to load deck");
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    void load();

    return () => {
      controller.abort();
    };
  }, [id, isAuthenticated]);

  const orderedCards = useMemo(
    () => [...(deck?.cards ?? [])].sort((a, b) => a.position - b.position),
    [deck?.cards],
  );

  const cardsNeedingReview = latestReview?.reviewCards.length ?? 0;
  const confidentCards = Math.max(orderedCards.length - cardsNeedingReview, 0);

  if (authLoading || loading) {
    return <main className="p-10">Loading deck...</main>;
  }

  if (error) {
    return <main className="p-10 text-destructive">{error}</main>;
  }

  if (!deck) {
    return <main className="p-10">Deck not found</main>;
  }

  const isOwner = user?.id === deck.ownerId;

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
            {isOwner ? (
              <>
                <Button asChild variant="outline">
                  <Link href={`/decks/${deck.id}/edit`}>Edit Deck</Link>
                </Button>

                <Button asChild variant="secondary" className="gap-2">
                  <Link href={`/decks/${deck.id}/test`}>
                    <ClipboardCheck className="h-4 w-4" />
                    Take Test
                  </Link>
                </Button>
              </>
            ) : null}

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
        <div>
          <h2 className="text-xl font-semibold">Latest study progress</h2>
          <p className="text-sm text-muted-foreground">
            This comes from the most recent saved study session for this deck.
          </p>
        </div>

        {latestReview ? (
          <>
            <div className="grid gap-4 md:grid-cols-3">
              <Card className="border-border/70 bg-card/70">
                <CardHeader className="gap-2">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Clock3 className="h-4 w-4" />
                    <span>Last reviewed</span>
                  </div>
                  <CardTitle className="text-xl">
                    {formatReviewedAt(latestReview.reviewedAt)}
                  </CardTitle>
                </CardHeader>
              </Card>

              <Card className="border-border/70 bg-card/70">
                <CardHeader className="gap-2">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <RefreshCcw className="h-4 w-4" />
                    <span>Need more review</span>
                  </div>
                  <CardTitle className="text-xl">
                    {cardsNeedingReview} card
                    {cardsNeedingReview === 1 ? "" : "s"}
                  </CardTitle>
                </CardHeader>
              </Card>

              <Card className="border-border/70 bg-card/70">
                <CardHeader className="gap-2">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Brain className="h-4 w-4" />
                    <span>Confident cards</span>
                  </div>
                  <CardTitle className="text-xl">
                    {confidentCards} card{confidentCards === 1 ? "" : "s"}
                  </CardTitle>
                </CardHeader>
              </Card>
            </div>

            <Card className="border-border/70 bg-card/70">
              <CardHeader>
                <CardTitle className="text-lg">Cards to revisit</CardTitle>
                <CardDescription>
                  {latestReview.reviewCards.length === 0
                    ? "You got everything right in the latest saved session."
                    : "These were marked for more review in the latest saved session."}
                </CardDescription>
              </CardHeader>

              <CardContent>
                {latestReview.reviewCards.length === 0 ? (
                  <p className="text-sm text-muted-foreground">
                    No cards need extra review right now.
                  </p>
                ) : (
                  <>
                    <div className="grid gap-3 md:grid-cols-2">
                      {latestReview.reviewCards.slice(0, 6).map((card) => (
                        <Card key={card.id} className="bg-background/60">
                          <CardHeader className="gap-2">
                            {card.termType === "image" ? (
                              // eslint-disable-next-line @next/next/no-img-element
                              <img src={card.term} alt="Term" className="max-h-32 w-full rounded object-contain" />
                            ) : (
                              <CardTitle className="text-base">
                                {card.term}
                              </CardTitle>
                            )}
                            {card.definitionType === "image" ? (
                              // eslint-disable-next-line @next/next/no-img-element
                              <img src={card.definition} alt="Definition" className="max-h-24 w-full rounded object-contain opacity-80" />
                            ) : (
                              <CardDescription className="line-clamp-2 text-sm leading-6 text-foreground/80">
                                {card.definition}
                              </CardDescription>
                            )}
                          </CardHeader>
                        </Card>
                      ))}
                    </div>

                    {latestReview.reviewCards.length > 6 ? (
                      <p className="mt-3 text-sm text-muted-foreground">
                        And {latestReview.reviewCards.length - 6} more card
                        {latestReview.reviewCards.length - 6 === 1 ? "" : "s"}.
                      </p>
                    ) : null}
                  </>
                )}
              </CardContent>
            </Card>
          </>
        ) : historyUnavailable ? (
          <Card className="border-border/70 bg-card/70">
            <CardContent className="py-10 text-center text-muted-foreground">
              Study history is unavailable right now.
            </CardContent>
          </Card>
        ) : (
          <Card className="border-border/70 bg-card/70">
            <CardContent className="py-10 text-center text-muted-foreground">
              No study history yet. Finish one study session to see saved
              progress here.
            </CardContent>
          </Card>
        )}
      </section>

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
                  {card.termType === "image" ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img src={card.term} alt="Term" className="max-h-32 w-full rounded object-contain" />
                  ) : (
                    <CardTitle className="text-lg">{card.term}</CardTitle>
                  )}
                  {card.definitionType === "image" ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img src={card.definition} alt="Definition" className="max-h-24 w-full rounded object-contain opacity-80" />
                  ) : (
                    <CardDescription className="line-clamp-3 text-sm leading-6 text-foreground/80">
                      {card.definition}
                    </CardDescription>
                  )}
                </CardHeader>
              </Card>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}