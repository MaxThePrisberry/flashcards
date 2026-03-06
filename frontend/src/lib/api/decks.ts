import type {
  DeckDetailDto,
  DeckSummaryDto,
  CreateDeckRequest,
  PaginatedResponse,
} from "@/lib/types";
import { apiFetch } from "@/lib/api/api-client";

export async function createDeck(
  data: CreateDeckRequest,
): Promise<DeckDetailDto> {
  const res = await apiFetch("/api/decks", {
    method: "POST",
    body: JSON.stringify(data),
  });

  return res.json();
}

export async function getDecks(): Promise<PaginatedResponse<DeckSummaryDto>> {
  const res = await apiFetch("/api/decks");
  return res.json();
}

export async function getDeck(id: string): Promise<DeckDetailDto> {
  const res = await apiFetch(`/api/decks/${id}`);
  return res.json();
}
