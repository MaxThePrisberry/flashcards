"use client";

import { useState, useRef, useCallback } from "react";
import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { CreateDeckRequest } from "@/lib/types";

type CardInput = {
  id: number;
  term: string;
  definition: string;
};

interface DeckFormProps {
  title?: string;
  submitLabel?: string;
  submittingLabel?: string;
  initialData?: {
    title: string;
    description: string;
    cards: { term: string; definition: string }[];
  };
  fieldErrors?: Record<string, string[]>;
  onSubmit: (data: CreateDeckRequest) => Promise<void>;
}

export default function DeckForm({
  title = "Create Deck",
  submitLabel = "Create Deck",
  submittingLabel = "Saving...",
  initialData,
  fieldErrors,
  onSubmit,
}: DeckFormProps) {
  const nextId = useRef(initialData?.cards.length ?? 1);

  const [deckTitle, setDeckTitle] = useState(initialData?.title ?? "");
  const [description, setDescription] = useState(initialData?.description ?? "");
  const [cards, setCards] = useState<CardInput[]>(
    initialData?.cards.map((c, i) => ({ id: i, ...c })) ?? [
      { id: 0, term: "", definition: "" },
    ],
  );
  const [isSubmitting, setIsSubmitting] = useState(false);

  const updateCard = useCallback(
    (id: number, field: "term" | "definition", value: string) => {
      setCards((prev) =>
        prev.map((card) =>
          card.id === id ? { ...card, [field]: value } : card,
        ),
      );
    },
    [],
  );

  function addCard() {
    setCards((prev) => [...prev, { id: nextId.current++, term: "", definition: "" }]);
  }

  function removeCard(id: number) {
    setCards((prev) => {
      if (prev.length <= 1) return prev;
      return prev.filter((card) => card.id !== id);
    });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      await onSubmit({
        title: deckTitle,
        description,
        cards: cards.map(({ term, definition }) => ({ term, definition })),
      });
    } finally {
      setIsSubmitting(false);
    }
  }

  function fieldError(field: string): string | undefined {
    return fieldErrors?.[field]?.[0];
  }

  return (
    <Card className="w-full max-w-xl">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit} className="flex flex-col gap-6">
          <div className="flex flex-col gap-2">
            <Label>Title</Label>
            <Input
              value={deckTitle}
              onChange={(e) => setDeckTitle(e.target.value)}
              required
              maxLength={200}
              disabled={isSubmitting}
            />
            {fieldError("title") && (
              <p className="text-sm text-destructive">{fieldError("title")}</p>
            )}
          </div>

          <div className="flex flex-col gap-2">
            <Label>Description</Label>
            <Input
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              maxLength={1000}
              disabled={isSubmitting}
            />
            {fieldError("description") && (
              <p className="text-sm text-destructive">{fieldError("description")}</p>
            )}
          </div>

          {cards.map((card, i) => (
            <div key={card.id} className="relative flex flex-col gap-2 border p-4 rounded-lg">
              {cards.length > 1 && (
                <button
                  type="button"
                  onClick={() => removeCard(card.id)}
                  disabled={isSubmitting}
                  aria-label={`Remove card ${i + 1}`}
                  className="absolute top-2 right-2 p-1 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition disabled:opacity-50"
                >
                  <X className="h-4 w-4" />
                </button>
              )}

              <Label>Term</Label>
              <Input
                value={card.term}
                onChange={(e) => updateCard(card.id, "term", e.target.value)}
                required
                maxLength={500}
                disabled={isSubmitting}
              />

              <Label>Definition</Label>
              <Input
                value={card.definition}
                onChange={(e) => updateCard(card.id, "definition", e.target.value)}
                required
                maxLength={2000}
                disabled={isSubmitting}
              />
            </div>
          ))}

          {fieldError("cards") && (
            <p className="text-sm text-destructive">{fieldError("cards")}</p>
          )}

          <Button type="button" variant="outline" onClick={addCard} disabled={isSubmitting}>
            Add Card
          </Button>

          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? submittingLabel : submitLabel}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
