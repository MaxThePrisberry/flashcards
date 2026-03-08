"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import DeckForm from "@/components/deck-form";
import type { DeckFormData } from "@/components/deck-form";
import { createDeck } from "@/lib/api/decks";
import { useRequireAuth } from "@/hooks/use-require-auth";
import { ApiError } from "@/lib/api/api-client";

export default function CreateDeckPage() {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useRequireAuth();
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  async function handleCreate(data: DeckFormData) {
    setError(null);
    setFieldErrors({});
    try {
      const deck = await createDeck(data);
      router.push(`/decks/${deck.id}`);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
        if (err.details) setFieldErrors(err.details);
      } else {
        setError("Failed to create deck");
      }
    }
  }

  if (isLoading || !isAuthenticated) {
    return <main className="p-10">Loading...</main>;
  }

  return (
    <main className="flex flex-col items-center px-4 py-10 gap-4">
      {error && <p className="text-sm text-destructive">{error}</p>}
      <DeckForm onSubmit={handleCreate} fieldErrors={fieldErrors} />
    </main>
  );
}
