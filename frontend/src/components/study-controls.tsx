"use client";

import { Check, RefreshCcw } from "lucide-react";
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
    <div className="grid gap-3 sm:grid-cols-2">
      <Button
        variant="outline"
        size="lg"
        onClick={() => onRate("needsReview")}
        disabled={disabled}
        className="gap-2"
      >
        <RefreshCcw className="h-4 w-4" />
        Needs review
      </Button>

      <Button
        size="lg"
        onClick={() => onRate("gotIt")}
        disabled={disabled}
        className="gap-2"
      >
        <Check className="h-4 w-4" />
        Got it
      </Button>
    </div>
  );
}
