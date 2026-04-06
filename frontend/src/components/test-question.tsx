"use client";

import { cn } from "@/lib/utils";
import { CheckCircle2, XCircle } from "lucide-react";
import type { TestQuestionDto } from "@/lib/types";
import { Card, CardContent, CardHeader } from "@/components/ui/card";

interface TestQuestionProps {
  question: TestQuestionDto;
  questionNumber: number;
  totalQuestions: number;
  selectedIndex: number | null;
  onSelect: (optionIndex: number) => void;
  phase: "idle" | "exiting" | "entering";
}

export default function TestQuestion({
  question,
  questionNumber,
  totalQuestions,
  selectedIndex,
  onSelect,
  phase,
}: TestQuestionProps) {
  const answered = selectedIndex !== null;
  const directionLabel =
    question.direction === "term_to_definition"
      ? "What is the definition?"
      : "What is the term?";

  return (
    <Card
      className={cn(
        "border-border/70 bg-card/90 transition-all duration-200 ease-out",
        phase === "idle" && "translate-x-0 opacity-100",
        phase === "exiting" && "-translate-x-8 opacity-0",
        phase === "entering" && "translate-x-8 opacity-0",
      )}
    >
      <CardHeader className="space-y-4 pb-4">
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>
            Question {questionNumber} of {totalQuestions}
          </span>
          <span className="rounded-full border border-border/70 px-3 py-1">
            {directionLabel}
          </span>
        </div>

        <p className="text-center text-2xl font-semibold leading-tight md:text-3xl">
          {question.prompt}
        </p>
      </CardHeader>

      <CardContent className="grid gap-3 sm:grid-cols-2">
        {question.options.map((option) => {
          const isSelected = selectedIndex === option.index;
          const isCorrect = option.index === question.correctOptionIndex;

          let state: "default" | "correct" | "wrong" | "missed" = "default";
          if (answered) {
            if (isSelected && isCorrect) state = "correct";
            else if (isSelected && !isCorrect) state = "wrong";
            else if (!isSelected && isCorrect) state = "missed";
          }

          return (
            <button
              key={option.index}
              onClick={() => onSelect(option.index)}
              disabled={answered}
              className={cn(
                "relative flex items-center gap-3 rounded-lg border px-4 py-3 text-left text-sm transition-colors",
                state === "default" &&
                  "border-border/70 bg-background/60 hover:bg-accent/40",
                state === "correct" &&
                  "border-green-500/70 bg-green-500/10 text-green-400",
                state === "wrong" &&
                  "border-red-500/70 bg-red-500/10 text-red-400",
                state === "missed" &&
                  "border-green-500/40 bg-green-500/5 text-green-400/70",
                answered && state === "default" && "opacity-50",
              )}
            >
              <span
                className={cn(
                  "flex h-7 w-7 shrink-0 items-center justify-center rounded-full border text-xs font-medium",
                  state === "default" && "border-border/70",
                  state === "correct" && "border-green-500/70 bg-green-500/20",
                  state === "wrong" && "border-red-500/70 bg-red-500/20",
                  state === "missed" && "border-green-500/40 bg-green-500/10",
                )}
              >
                {state === "correct" ? (
                  <CheckCircle2 className="h-4 w-4" />
                ) : state === "wrong" ? (
                  <XCircle className="h-4 w-4" />
                ) : state === "missed" ? (
                  <CheckCircle2 className="h-4 w-4" />
                ) : (
                  String.fromCharCode(65 + option.index)
                )}
              </span>

              <span className="flex-1">{option.text}</span>
            </button>
          );
        })}
      </CardContent>
    </Card>
  );
}
