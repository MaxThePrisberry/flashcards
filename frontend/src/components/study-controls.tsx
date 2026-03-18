"use client";

import { RefreshCcw, Check, Sparkles } from "lucide-react";
import type { StudyRating } from "@/hooks/use-study-session";
import { Button } from "@/components/ui/button";

interface StudyControlsProps {
  onRate: (rating: StudyRating) => void;
  disabled?: boolean;
}

export default function StudyControls({
  onRate,
  disabled = false,
}: StudyControlsProps) {
  return (
    <div className="grid gap-3 sm:grid-cols-3">
      <Button
        variant="outline"
        size="lg"
        onClick={() => onRate("again")}
        disabled={disabled}
        className="gap-2"
      >
        <RefreshCcw className="h-4 w-4" />
        Needs Practice
      </Button>

      <Button
        variant="secondary"
        size="lg"
        onClick={() => onRate("good")}
        disabled={disabled}
        className="gap-2"
      >
        <Check className="h-4 w-4" />
        Almost There
      </Button>

      <Button
        size="lg"
        onClick={() => onRate("easy")}
        disabled={disabled}
        className="gap-2"
      >
        <Sparkles className="h-4 w-4" />I Know It
      </Button>
    </div>
  );
}
