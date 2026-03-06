"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

type CardInput = {
  term: string;
  definition: string;
};

export default function DeckForm({
  onSubmit,
}: {
  onSubmit: (data: {
    title: string;
    description: string;
    cards: CardInput[];
  }) => Promise<void>;
}) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [cards, setCards] = useState<CardInput[]>([
    { term: "", definition: "" },
  ]);

  function updateCard(index: number, field: keyof CardInput, value: string) {
    const updated = [...cards];
    updated[index][field] = value;
    setCards(updated);
  }

  function addCard() {
    setCards([...cards, { term: "", definition: "" }]);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    await onSubmit({ title, description, cards });
  }

  return (
    <Card className="w-full max-w-xl">
      <CardHeader>
        <CardTitle>Create Deck</CardTitle>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit} className="flex flex-col gap-6">
          <div className="flex flex-col gap-2">
            <Label>Title</Label>
            <Input
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label>Description</Label>
            <Input
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          {cards.map((card, i) => (
            <div key={i} className="flex flex-col gap-2 border p-4 rounded-lg">
              <Label>Term</Label>
              <Input
                value={card.term}
                onChange={(e) => updateCard(i, "term", e.target.value)}
                required
              />

              <Label>Definition</Label>
              <Input
                value={card.definition}
                onChange={(e) => updateCard(i, "definition", e.target.value)}
                required
              />
            </div>
          ))}

          <Button type="button" variant="outline" onClick={addCard}>
            Add Card
          </Button>

          <Button type="submit">Create Deck</Button>
        </form>
      </CardContent>
    </Card>
  );
}
