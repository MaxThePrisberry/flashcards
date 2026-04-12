"use client";

import { RotateCcw } from "lucide-react";
import { cn } from "@/lib/utils";
import type { CardDto } from "@/lib/types";
import { Card, CardContent } from "@/components/ui/card";

type CardPhase = "idle" | "exiting" | "entering";

interface StudyCardProps {
  card: CardDto | null;
  flipped: boolean;
  onFlip: () => void;
  phase: CardPhase;
}

export default function StudyCard({
  card,
  flipped,
  onFlip,
  phase,
}: StudyCardProps) {
  if (!card) return null;

  return (
    <div className="[perspective:1200px]">
      <Card
        onClick={onFlip}
        className={cn(
          "cursor-pointer overflow-hidden border-border/70 bg-card/90 transition-all duration-200 ease-out select-none",
          phase === "idle" && "translate-x-0 opacity-100",
          phase === "exiting" && "-translate-x-8 opacity-0",
          phase === "entering" && "translate-x-8 opacity-0",
        )}
      >
        <CardContent className="p-0">
          <div
            className={cn(
              "relative min-h-[26rem] w-full transition-transform duration-300 [transform-style:preserve-3d]",
              flipped && "[transform:rotateY(180deg)]",
            )}
          >
            <div className="absolute inset-0 flex flex-col justify-between p-8 [backface-visibility:hidden]">
              <div className="flex items-center justify-between text-sm text-muted-foreground">
                <span>Front</span>
                <span className="rounded-full border border-border/70 px-3 py-1">
                  Tap to reveal
                </span>
              </div>

              <div className="flex flex-1 items-center justify-center">
                {card.termType === "image" ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img src={card.term} alt="Term" className="max-h-56 max-w-full rounded object-contain" />
                ) : (
                  <p className="max-w-2xl text-center text-3xl font-semibold leading-tight md:text-5xl">
                    {card.term}
                  </p>
                )}
              </div>

              <div className="flex items-center justify-center gap-2 text-sm text-muted-foreground">
                <RotateCcw className="h-4 w-4" />
                <span>Press Space or click anywhere on the card</span>
              </div>
            </div>

            <div className="absolute inset-0 flex flex-col justify-between p-8 [backface-visibility:hidden] [transform:rotateY(180deg)]">
              <div className="flex items-center justify-between text-sm text-muted-foreground">
                <span>Back</span>
                <span className="rounded-full border border-border/70 px-3 py-1">
                  Answer revealed
                </span>
              </div>

              <div className="flex flex-1 items-center justify-center">
                {card.definitionType === "image" ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img src={card.definition} alt="Definition" className="max-h-56 max-w-full rounded object-contain" />
                ) : (
                  <p className="max-w-2xl text-center text-2xl leading-relaxed text-foreground/90 md:text-4xl">
                    {card.definition}
                  </p>
                )}
              </div>

              <div className="text-center text-sm text-muted-foreground">
                Choose Got it or Needs review to continue
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
