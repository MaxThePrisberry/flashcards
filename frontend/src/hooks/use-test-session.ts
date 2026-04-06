"use client";

import { useCallback, useState } from "react";
import type { TestQuestionDto } from "@/lib/types";

export interface TestAnswer {
  cardId: string;
  selectedIndex: number;
  correctIndex: number;
  correct: boolean;
}

export function useTestSession(questions: TestQuestionDto[]) {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [answers, setAnswers] = useState<TestAnswer[]>([]);
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);

  const totalQuestions = questions.length;
  const currentQuestion = questions[currentIndex] ?? null;
  const completed = totalQuestions > 0 && currentIndex >= totalQuestions;
  const answeredCount = answers.length;
  const correctCount = answers.filter((a) => a.correct).length;
  const wrongCardIds = answers.filter((a) => !a.correct).map((a) => a.cardId);

  const selectAnswer = useCallback(
    (optionIndex: number) => {
      if (selectedIndex !== null || !currentQuestion) return;

      const correct = optionIndex === currentQuestion.correctOptionIndex;

      setSelectedIndex(optionIndex);
      setAnswers((prev) => [
        ...prev,
        {
          cardId: currentQuestion.cardId,
          selectedIndex: optionIndex,
          correctIndex: currentQuestion.correctOptionIndex,
          correct,
        },
      ]);
    },
    [currentQuestion, selectedIndex],
  );

  const advance = useCallback(() => {
    setSelectedIndex(null);
    setCurrentIndex((prev) => prev + 1);
  }, []);

  const reset = useCallback(() => {
    setCurrentIndex(0);
    setAnswers([]);
    setSelectedIndex(null);
  }, []);

  return {
    currentQuestion,
    currentIndex,
    totalQuestions,
    completed,
    answeredCount,
    correctCount,
    wrongCardIds,
    selectedIndex,
    selectAnswer,
    advance,
    reset,
  };
}
