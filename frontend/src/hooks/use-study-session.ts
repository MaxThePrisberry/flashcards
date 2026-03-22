"use client";

import { useMemo, useState } from "react";
import type { CardDto } from "@/lib/types";

export type StudyRating = "needsReview" | "gotIt";

interface RatingCounts {
  needsReview: number;
  gotIt: number;
}

export function useStudySession(cards: CardDto[]) {
  const orderedCards = useMemo(
    () => [...cards].sort((a, b) => a.position - b.position),
    [cards],
  );

  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [ratings, setRatings] = useState<RatingCounts>({
    needsReview: 0,
    gotIt: 0,
  });
  const [needsReviewIds, setNeedsReviewIds] = useState<string[]>([]);

  const totalCards = orderedCards.length;
  const currentCard = orderedCards[currentIndex] ?? null;
  const completed = totalCards > 0 && currentIndex >= totalCards;
  const reviewedCount = Math.min(currentIndex, totalCards);
  const currentCardNumber = completed
    ? totalCards
    : Math.min(currentIndex + 1, totalCards);

  function flipCard() {
    if (completed || totalCards === 0) return;
    setIsFlipped((prev) => !prev);
  }

  function rateCard(rating: StudyRating) {
    if (completed || !currentCard) return;

    setRatings((prev) => ({
      ...prev,
      [rating]: prev[rating] + 1,
    }));

    if (rating === "needsReview") {
      setNeedsReviewIds((prev) => [...prev, currentCard.id]);
    }

    setIsFlipped(false);
    setCurrentIndex((prev) => prev + 1);
  }

  function resetSession() {
    setCurrentIndex(0);
    setIsFlipped(false);
    setRatings({
      needsReview: 0,
      gotIt: 0,
    });
    setNeedsReviewIds([]);
  }

  return {
    currentCard,
    currentIndex,
    currentCardNumber,
    totalCards,
    isFlipped,
    completed,
    reviewedCount,
    ratings,
    needsReviewIds,
    flipCard,
    rateCard,
    resetSession,
  };
}
