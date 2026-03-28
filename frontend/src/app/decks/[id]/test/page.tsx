"use client";

import Link from "next/link";
import { use, useCallback, useEffect, useRef, useState } from "react";
import {
  ArrowLeft,
  CheckCircle2,
  ClipboardCheck,
  Loader2,
  RotateCcw,
} from "lucide-react";
import { generateTest, submitDeckReview } from "@/lib/api/decks";
import { ApiError } from "@/lib/api/api-client";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { useTestSession } from "@/hooks/use-test-session";
import TestQuestion from "@/components/test-question";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import type { TestResponse } from "@/lib/types";

type CardPhase = "idle" | "exiting" | "entering";

export default function TestDeckPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { isAuthenticated, isLoading: authLoading } = useRequireAuth();

  const [test, setTest] = useState<TestResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [phase, setPhase] = useState<CardPhase>("idle");
  const [reviewSubmitted, setReviewSubmitted] = useState(false);

  const transitionTimeoutRef = useRef<number | null>(null);
  const enterFrameRef = useRef<number | null>(null);

  const loadTest = useCallback(
    async (signal?: AbortSignal) => {
      setLoading(true);
      setError(null);
      try {
        const data = await generateTest(id, signal);
        if (signal?.aborted) return;
        setTest(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") return;
        if (signal?.aborted) return;
        setError(
          err instanceof ApiError ? err.message : "Failed to generate test",
        );
      } finally {
        if (!signal?.aborted) setLoading(false);
      }
    },
    [id],
  );

  useEffect(() => {
    if (!isAuthenticated) return;

    const controller = new AbortController();
    loadTest(controller.signal);

    return () => {
      controller.abort();
      if (transitionTimeoutRef.current !== null)
        window.clearTimeout(transitionTimeoutRef.current);
      if (enterFrameRef.current !== null)
        window.cancelAnimationFrame(enterFrameRef.current);
    };
  }, [isAuthenticated, loadTest]);

  const {
    currentQuestion,
    currentIndex,
    totalQuestions,
    completed,
    correctCount,
    wrongCardIds,
    selectedIndex,
    selectAnswer,
    advance,
    reset,
  } = useTestSession(test?.questions ?? []);

  // Auto-submit wrong answers as review when test completes
  useEffect(() => {
    if (!completed || reviewSubmitted || !test) return;

    setReviewSubmitted(true);
    submitDeckReview(test.deckId, { needsReview: wrongCardIds }).catch(() => {
      // Silent failure — review submission is best-effort
    });
  }, [completed, reviewSubmitted, test, wrongCardIds]);

  const handleSelect = useCallback(
    (optionIndex: number) => {
      if (phase !== "idle") return;
      selectAnswer(optionIndex);
    },
    [phase, selectAnswer],
  );

  const handleContinue = useCallback(() => {
    if (phase !== "idle" || selectedIndex === null) return;

    setPhase("exiting");

    transitionTimeoutRef.current = window.setTimeout(() => {
      advance();
      setPhase("entering");

      enterFrameRef.current = window.requestAnimationFrame(() => {
        enterFrameRef.current = window.requestAnimationFrame(() => {
          setPhase("idle");
        });
      });
    }, 180);
  }, [advance, phase, selectedIndex]);

  // Keyboard: Enter/Space to continue after answering, 1-4 to select option
  useEffect(() => {
    if (completed || totalQuestions === 0) return;

    function onKeyDown(event: KeyboardEvent) {
      const target = event.target as HTMLElement | null;
      if (target?.tagName === "INPUT" || target?.tagName === "TEXTAREA") return;

      if (selectedIndex !== null) {
        if (event.code === "Space" || event.key === "Enter") {
          event.preventDefault();
          handleContinue();
          return;
        }
      }

      if (selectedIndex === null && currentQuestion) {
        const num = parseInt(event.key, 10);
        if (num >= 1 && num <= currentQuestion.options.length) {
          event.preventDefault();
          handleSelect(num - 1);
        }
      }
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [
    completed,
    currentQuestion,
    handleContinue,
    handleSelect,
    selectedIndex,
    totalQuestions,
  ]);

  const handleRetake = useCallback(async () => {
    reset();
    setReviewSubmitted(false);
    setTest(null);
    await loadTest();
  }, [loadTest, reset]);

  if (authLoading || loading) {
    return (
      <main className="flex min-h-[60vh] flex-col items-center justify-center gap-4 p-10">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        <p className="text-muted-foreground">
          {loading ? "Generating test questions..." : "Loading..."}
        </p>
      </main>
    );
  }

  if (error) {
    return (
      <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-6 py-8">
        <Button asChild variant="ghost" className="w-fit gap-2">
          <Link href={`/decks/${id}`}>
            <ArrowLeft className="h-4 w-4" />
            Back to deck
          </Link>
        </Button>
        <Card className="border-destructive/50">
          <CardHeader>
            <CardTitle className="text-destructive">
              Failed to generate test
            </CardTitle>
            <CardDescription>{error}</CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={() => loadTest()} variant="outline">
              Try again
            </Button>
          </CardContent>
        </Card>
      </main>
    );
  }

  if (!test || totalQuestions === 0) {
    return (
      <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-6 py-8">
        <Button asChild variant="ghost" className="w-fit gap-2">
          <Link href={`/decks/${id}`}>
            <ArrowLeft className="h-4 w-4" />
            Back to deck
          </Link>
        </Button>
        <Card>
          <CardHeader>
            <CardTitle>{test?.deckTitle ?? "Test"}</CardTitle>
            <CardDescription>
              Add cards before starting a test.
            </CardDescription>
          </CardHeader>
        </Card>
      </main>
    );
  }

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-6 py-8">
      <div className="flex items-center justify-between gap-3">
        <Button asChild variant="ghost" className="w-fit gap-2">
          <Link href={`/decks/${id}`}>
            <ArrowLeft className="h-4 w-4" />
            Back to deck
          </Link>
        </Button>

        {!completed && (
          <div className="hidden items-center gap-2 text-sm text-muted-foreground md:flex">
            <span>1-4 to select</span>
            <span>·</span>
            <span>Enter to continue</span>
          </div>
        )}
      </div>

      {/* Progress bar */}
      <Card className="border-border/70 bg-card/80">
        <CardContent className="space-y-5 p-6">
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">Test mode</p>
            <h1 className="text-3xl font-semibold">{test.deckTitle}</h1>
          </div>

          <div className="flex items-center justify-between gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Progress</p>
              <p className="text-lg font-medium">
                {completed
                  ? `${totalQuestions} / ${totalQuestions}`
                  : `${currentIndex + 1} / ${totalQuestions}`}
              </p>
            </div>
            <p className="text-sm text-muted-foreground">
              {completed
                ? `${correctCount} / ${totalQuestions} correct`
                : `${correctCount} correct so far`}
            </p>
          </div>

          <div className="grid grid-cols-12 gap-2 sm:grid-cols-[repeat(auto-fit,minmax(0,1fr))]">
            {test.questions.map((_, index) => {
              const isDone = index < currentIndex;
              const isCurrent = !completed && index === currentIndex;

              return (
                <div
                  key={index}
                  className={
                    "h-2 rounded-full transition-colors " +
                    (isDone
                      ? "bg-primary"
                      : isCurrent
                        ? "bg-primary/50"
                        : "bg-muted")
                  }
                />
              );
            })}
          </div>
        </CardContent>
      </Card>

      {completed ? (
        <Card className="border-border/70 bg-card/90">
          <CardHeader className="items-center text-center">
            <div className="flex h-14 w-14 items-center justify-center rounded-full bg-primary/10 text-primary">
              <CheckCircle2 className="h-7 w-7" />
            </div>
            <CardTitle className="text-3xl">Test complete</CardTitle>
            <CardDescription className="text-base">
              You scored {correctCount} out of {totalQuestions} (
              {Math.round((correctCount / totalQuestions) * 100)}%)
            </CardDescription>
          </CardHeader>

          <CardContent className="space-y-6">
            <div className="grid gap-4 sm:grid-cols-2">
              <Card className="bg-background/60">
                <CardHeader className="pb-2">
                  <CardDescription>Correct</CardDescription>
                  <CardTitle className="text-3xl text-green-400">
                    {correctCount}
                  </CardTitle>
                </CardHeader>
              </Card>

              <Card className="bg-background/60">
                <CardHeader className="pb-2">
                  <CardDescription>Incorrect</CardDescription>
                  <CardTitle className="text-3xl text-red-400">
                    {totalQuestions - correctCount}
                  </CardTitle>
                </CardHeader>
              </Card>
            </div>

            <div className="flex flex-col justify-center gap-3 sm:flex-row">
              <Button onClick={handleRetake} size="lg" className="gap-2">
                <RotateCcw className="h-4 w-4" />
                Retake Test
              </Button>

              <Button asChild variant="outline" size="lg" className="gap-2">
                <Link href={`/decks/${id}`}>
                  <ClipboardCheck className="h-4 w-4" />
                  Back to deck
                </Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : (
        <>
          {currentQuestion && (
            <TestQuestion
              question={currentQuestion}
              questionNumber={currentIndex + 1}
              totalQuestions={totalQuestions}
              selectedIndex={selectedIndex}
              onSelect={handleSelect}
              phase={phase}
            />
          )}

          <div className="flex justify-center">
            {selectedIndex !== null && (
              <Button size="lg" onClick={handleContinue}>
                {currentIndex + 1 < totalQuestions
                  ? "Next Question"
                  : "See Results"}
              </Button>
            )}
          </div>
        </>
      )}
    </main>
  );
}
