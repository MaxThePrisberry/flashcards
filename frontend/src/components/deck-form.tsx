"use client";

import { useState, useRef, useCallback } from "react";
import { X, ImageIcon, Type } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

type ItemType = "text" | "image";

type CardInput = {
  id: number;
  term: string;
  definition: string;
  termType: ItemType;
  definitionType: ItemType;
};

export type DeckFormData = {
  title: string;
  description: string;
  cards: { term: string; definition: string; termType: string; definitionType: string }[];
};

interface DeckFormProps {
  title?: string;
  submitLabel?: string;
  submittingLabel?: string;
  initialData?: {
    title: string;
    description: string;
    cards: { term: string; definition: string; termType?: string; definitionType?: string }[];
  };
  fieldErrors?: Record<string, string[]>;
  onSubmit: (data: DeckFormData) => Promise<void>;
}

function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

interface CardFieldProps {
  label: string;
  cardId: number;
  field: "term" | "definition";
  value: string;
  type: ItemType;
  disabled: boolean;
  onTypeChange: (id: number, field: "term" | "definition", type: ItemType) => void;
  onValueChange: (id: number, field: "term" | "definition", value: string) => void;
}

function CardField({ label, cardId, field, value, type, disabled, onTypeChange, onValueChange }: CardFieldProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);

  async function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    const base64 = await fileToBase64(file);
    onValueChange(cardId, field, base64);
  }

  return (
    <div className="flex flex-col gap-1">
      <div className="flex items-center justify-between">
        <Label>{label}</Label>
        <div className="flex gap-1">
          <button
            type="button"
            disabled={disabled}
            onClick={() => onTypeChange(cardId, field, "text")}
            aria-label={`${label}: text`}
            className={`rounded p-1 text-xs transition ${type === "text" ? "bg-accent text-foreground" : "text-muted-foreground hover:text-foreground"}`}
          >
            <Type className="h-3.5 w-3.5" />
          </button>
          <button
            type="button"
            disabled={disabled}
            onClick={() => onTypeChange(cardId, field, "image")}
            aria-label={`${label}: image`}
            className={`rounded p-1 text-xs transition ${type === "image" ? "bg-accent text-foreground" : "text-muted-foreground hover:text-foreground"}`}
          >
            <ImageIcon className="h-3.5 w-3.5" />
          </button>
        </div>
      </div>

      {type === "text" ? (
        <Input
          value={value}
          onChange={(e) => onValueChange(cardId, field, e.target.value)}
          required
          disabled={disabled}
        />
      ) : (
        <div className="flex flex-col gap-2">
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            disabled={disabled}
            onChange={handleFileChange}
          />
          {value ? (
            <div className="relative">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={value}
                alt={`${label} preview`}
                className="max-h-40 w-full rounded border border-border object-contain"
              />
              <button
                type="button"
                disabled={disabled}
                onClick={() => onValueChange(cardId, field, "")}
                className="absolute right-1 top-1 rounded-full bg-background/80 p-0.5 text-muted-foreground hover:text-foreground"
                aria-label="Remove image"
              >
                <X className="h-3.5 w-3.5" />
              </button>
            </div>
          ) : (
            <button
              type="button"
              disabled={disabled}
              onClick={() => fileInputRef.current?.click()}
              className="flex h-20 w-full items-center justify-center gap-2 rounded border border-dashed border-border text-sm text-muted-foreground hover:border-foreground/40 hover:text-foreground transition"
            >
              <ImageIcon className="h-4 w-4" />
              Upload image
            </button>
          )}
          {/* hidden required sentinel so form validation fires when image is missing */}
          <input type="text" value={value} required readOnly className="sr-only" aria-hidden />
        </div>
      )}
    </div>
  );
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
    initialData?.cards.map((c, i) => ({
      id: i,
      term: c.term,
      definition: c.definition,
      termType: (c.termType === "image" ? "image" : "text") as ItemType,
      definitionType: (c.definitionType === "image" ? "image" : "text") as ItemType,
    })) ?? [{ id: 0, term: "", definition: "", termType: "text", definitionType: "text" }],
  );
  const [isSubmitting, setIsSubmitting] = useState(false);

  const updateCardValue = useCallback(
    (id: number, field: "term" | "definition", value: string) => {
      setCards((prev) =>
        prev.map((card) => (card.id === id ? { ...card, [field]: value } : card)),
      );
    },
    [],
  );

  const updateCardType = useCallback(
    (id: number, field: "term" | "definition", type: ItemType) => {
      const typeField = field === "term" ? "termType" : "definitionType";
      setCards((prev) =>
        prev.map((card) =>
          card.id === id ? { ...card, [typeField]: type, [field]: "" } : card,
        ),
      );
    },
    [],
  );

  function addCard() {
    setCards((prev) => [
      ...prev,
      { id: nextId.current++, term: "", definition: "", termType: "text", definitionType: "text" },
    ]);
  }

  function removeCard(id: number) {
    setCards((prev) => {
      if (prev.length <= 1) return prev;
      return prev.filter((card) => card.id !== id);
    });
  }

  async function handleSubmit(e: React.SyntheticEvent<HTMLFormElement>) {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      await onSubmit({
        title: deckTitle,
        description,
        cards: cards.map(({ term, definition, termType, definitionType }) => ({
          term,
          definition,
          termType,
          definitionType,
        })),
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
            <Textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              maxLength={1000}
              disabled={isSubmitting}
              rows={3}
            />
            {fieldError("description") && (
              <p className="text-sm text-destructive">{fieldError("description")}</p>
            )}
          </div>

          {cards.map((card, i) => (
            <div key={card.id} className="relative flex flex-col gap-3 border p-4 rounded-lg">
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

              <CardField
                label="Term"
                cardId={card.id}
                field="term"
                value={card.term}
                type={card.termType}
                disabled={isSubmitting}
                onTypeChange={updateCardType}
                onValueChange={updateCardValue}
              />

              <CardField
                label="Definition"
                cardId={card.id}
                field="definition"
                value={card.definition}
                type={card.definitionType}
                disabled={isSubmitting}
                onTypeChange={updateCardType}
                onValueChange={updateCardValue}
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
