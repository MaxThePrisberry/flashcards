"use client";

import { useRouter } from "next/navigation";
import DeckForm from "@/components/deck-form";
import { createDeck } from "@/lib/api/decks";

export default function CreateDeckPage() {
  const router = useRouter();

  async function handleCreate(data: any) {
    const deck = await createDeck(data);
    router.push(`/decks/${deck.id}`);
  }

  return (
    <main className="flex justify-center px-4 py-10">
      <DeckForm onSubmit={handleCreate} />
    </main>
  );
}
