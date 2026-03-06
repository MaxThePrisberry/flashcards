"use client";

import { useState } from "react";
import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { CreateDeckRequest } from "@/lib/types";

type CardInput = {
  term: string;
  definition: string;
};

export default function DeckForm({
  title = "Create Deck",
  onSubmit,
}: {
  title?: string;
  onSubmit: (data: CreateDeckRequest) => Promise<void>;
}) {
  const [deckTitle, setDeckTitle] = useState("");
  const [description, setDescription] = useState("");
  const [cards, setCards] = useState<CardInput[]>([
    { term: "", definition: "" },
  ]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function updateCard(index: number, field: keyof CardInput, value: string) {
    setCards((prev) =>
      prev.map((card, i) =>
        i === index ? { ...card, [field]: value } : card,
      ),
    );
  }

  function addCard() {
    setCards([...cards, { term: "", definition: "" }]);
  }

  function removeCard(index: number) {
    if (cards.length <= 1) return;
    setCards((prev) => prev.filter((_, i) => i !== index));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      await onSubmit({ title: deckTitle, description, cards });
    } finally {
      setIsSubmitting(false);
    }
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
              disabled={isSubmitting}
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label>Description</Label>
            <Input
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              disabled={isSubmitting}
            />
          </div>

          {cards.map((card, i) => (
            <div key={i} className="relative flex flex-col gap-2 border p-4 rounded-lg">
              {cards.length > 1 && (
                <button
                  type="button"
                  onClick={() => removeCard(i)}
                  disabled={isSubmitting}
                  className="absolute top-2 right-2 p-1 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition disabled:opacity-50"
                >
                  <X className="h-4 w-4" />
                </button>
              )}

              <Label>Term</Label>
              <Input
                value={card.term}
                onChange={(e) => updateCard(i, "term", e.target.value)}
                required
                disabled={isSubmitting}
              />

              <Label>Definition</Label>
              <Input
                value={card.definition}
                onChange={(e) => updateCard(i, "definition", e.target.value)}
                required
                disabled={isSubmitting}
              />
            </div>
          ))}

          <Button type="button" variant="outline" onClick={addCard} disabled={isSubmitting}>
            Add Card
          </Button>

          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating..." : "Create Deck"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
