"use client";

import Link from "next/link";
import { use, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { ArrowLeft, Brain, CheckCircle2, RotateCcw } from "lucide-react";
import { toast } from "sonner";
import { getDeck, submitDeckReview } from "@/lib/api/decks";
import { ApiError } from "@/lib/api/api-client";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { useStudySession, type StudyRating } from "@/hooks/use-study-session";
import StudyCard from "@/components/study-card";
import StudyControls from "@/components/study-controls";
import StudyProgress from "@/components/study-progress";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import type { DeckDetailDto, ReviewSessionDto } from "@/lib/types";

type CardPhase = "idle" | "exiting" | "entering";

export default function StudyDeckPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { isAuthenticated, isLoading: authLoading } = useRequireAuth();

  const [deck, setDeck] = useState<DeckDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [phase, setPhase] = useState<CardPhase>("idle");
  const [reviewSession, setReviewSession] = useState<ReviewSessionDto | null>(
    null,
  );
  const [submittingReview, setSubmittingReview] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const hasSubmittedRef = useRef(false);

  const transitionTimeoutRef = useRef<number | null>(null);
  const enterFrameRef = useRef<number | null>(null);

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

      if (transitionTimeoutRef.current !== null) {
        window.clearTimeout(transitionTimeoutRef.current);
      }

      if (enterFrameRef.current !== null) {
        window.cancelAnimationFrame(enterFrameRef.current);
      }
    };
  }, [id, isAuthenticated]);

  const orderedCards = useMemo(
    () => [...(deck?.cards ?? [])].sort((a, b) => a.position - b.position),
    [deck?.cards],
  );

  const {
    currentCard,
    currentIndex,
    currentCardNumber,
    totalCards,
    isFlipped,
    completed,
    ratings,
    reviewedCount,
    needsReviewIds,
    flipCard,
    rateCard,
    resetSession,
  } = useStudySession(orderedCards);

  const handleFlip = useCallback(() => {
    if (phase !== "idle" || completed || totalCards === 0) return;
    flipCard();
  }, [completed, flipCard, phase, totalCards]);

  const handleRate = useCallback(
    (rating: StudyRating) => {
      if (phase !== "idle" || !isFlipped || completed) return;

      setPhase("exiting");

      transitionTimeoutRef.current = window.setTimeout(() => {
        rateCard(rating);
        setPhase("entering");

        enterFrameRef.current = window.requestAnimationFrame(() => {
          enterFrameRef.current = window.requestAnimationFrame(() => {
            setPhase("idle");
          });
        });
      }, 180);
    },
    [completed, isFlipped, phase, rateCard],
  );

  const handleResetSession = useCallback(() => {
    hasSubmittedRef.current = false;
    setReviewSession(null);
    setSubmittingReview(false);
    setSubmitError(null);
    resetSession();
  }, [resetSession]);

  const needsReviewCount =
    reviewSession?.reviewCards.length ?? ratings.needsReview;

  useEffect(() => {
    if (completed || totalCards === 0) return;

    function onKeyDown(event: KeyboardEvent) {
      const target = event.target as HTMLElement | null;
      const tagName = target?.tagName;

      if (tagName === "INPUT" || tagName === "TEXTAREA") return;

      if (event.code === "Space" || event.key === "Enter") {
        event.preventDefault();
        handleFlip();
        return;
      }

      if (!isFlipped) return;

      if (event.key === "1") {
        event.preventDefault();
        handleRate("needsReview");
      } else if (event.key === "2") {
        event.preventDefault();
        handleRate("gotIt");
      }
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [completed, handleFlip, handleRate, isFlipped, totalCards]);

  useEffect(() => {
    if (!completed || !deck || hasSubmittedRef.current) return;

    hasSubmittedRef.current = true;
    const deckId = deck.id;
    let cancelled = false;

    async function saveReviewSession() {
      setSubmittingReview(true);
      setSubmitError(null);

      try {
        const session = await submitDeckReview(deckId, {
          needsReview: needsReviewIds,
        });

        if (!cancelled) {
          setReviewSession(session);
          toast.success("Study session saved!");
        }
      } catch (err) {
        if (!cancelled) {
          const message =
            err instanceof ApiError
              ? err.message
              : "Failed to save study session";
          setSubmitError(message);
          toast.error(message);
        }
      } finally {
        if (!cancelled) {
          setSubmittingReview(false);
        }
      }
    }

    saveReviewSession();

    return () => {
      cancelled = true;
    };
  }, [completed, deck, needsReviewIds]);

  if (authLoading || loading) {
    return <main className="p-10">Loading study session...</main>;
  }

  if (error) {
    return <main className="p-10 text-destructive">{error}</main>;
  }

  if (!deck) {
    return <main className="p-10">Deck not found</main>;
  }

  if (orderedCards.length === 0) {
    return (
      <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-6 py-8">
        <Button asChild variant="ghost" className="w-fit gap-2">
          <Link href={`/decks/${deck.id}`}>
            <ArrowLeft className="h-4 w-4" />
            Back to deck
          </Link>
        </Button>

        <Card>
          <CardHeader>
            <CardTitle>{deck.title}</CardTitle>
            <CardDescription>
              Add cards before starting study mode.
            </CardDescription>
          </CardHeader>
        </Card>
      </main>
    );
  }

  return (
    <main className="mx-auto flex w-full max-w-4xl flex-col gap-6 px-6 py-8">
      <div className="flex items-center justify-between gap-3">
        <Button asChild variant="ghost" className="w-fit gap-2">
          <Link href={`/decks/${deck.id}`}>
            <ArrowLeft className="h-4 w-4" />
            Back to deck
          </Link>
        </Button>

        <div className="hidden items-center gap-2 text-sm text-muted-foreground md:flex">
          <span>Space / Enter to flip</span>
          <span>•</span>
          <span>1 Needs review</span>
          <span>•</span>
          <span>2 Got it</span>
        </div>
      </div>

      <StudyProgress
        title={deck.title}
        description={deck.description}
        currentIndex={currentIndex}
        currentCardNumber={currentCardNumber}
        totalCards={totalCards}
        completed={completed}
      />

      {completed ? (
        <Card className="border-border/70 bg-card/90">
          <CardHeader className="items-center text-center">
            <div className="flex h-14 w-14 items-center justify-center rounded-full bg-primary/10 text-primary">
              <CheckCircle2 className="h-7 w-7" />
            </div>
            <CardTitle className="text-3xl">Deck complete</CardTitle>
            <CardDescription className="text-base">
              You reviewed all {totalCards} cards in this deck.
            </CardDescription>
          </CardHeader>

          <CardContent className="space-y-6">
            <div className="grid gap-4 sm:grid-cols-2">
              <Card className="bg-background/60">
                <CardHeader className="pb-2">
                  <CardDescription>Needs review</CardDescription>
                  <CardTitle className="text-3xl">{needsReviewCount}</CardTitle>
                </CardHeader>
              </Card>

              <Card className="bg-background/60">
                <CardHeader className="pb-2">
                  <CardDescription>Got it</CardDescription>
                  <CardTitle className="text-3xl">{ratings.gotIt}</CardTitle>
                </CardHeader>
              </Card>
            </div>

            <div className="flex flex-col justify-center gap-3 sm:flex-row">
              <Button onClick={handleResetSession} size="lg" className="gap-2">
                <RotateCcw className="h-4 w-4" />
                Study again
              </Button>

              <Button asChild variant="outline" size="lg" className="gap-2">
                <Link href={`/decks/${deck.id}`}>
                  <Brain className="h-4 w-4" />
                  Back to deck
                </Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : (
        <>
          <StudyCard
            card={currentCard}
            flipped={isFlipped}
            onFlip={handleFlip}
            phase={phase}
          />

          <div className="space-y-4">
            {!isFlipped ? (
              <div className="flex justify-center">
                <Button size="lg" onClick={handleFlip}>
                  Flip Card
                </Button>
              </div>
            ) : (
              <StudyControls onRate={handleRate} disabled={phase !== "idle"} />
            )}

            <p className="text-center text-sm text-muted-foreground">
              Card {currentCardNumber} of {totalCards} • {reviewedCount}{" "}
              reviewed
            </p>
          </div>
        </>
      )}
    </main>
  );
}
