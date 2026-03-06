import type {
  DeckDetailDto,
  DeckSummaryDto,
  CreateDeckRequest,
  UpdateDeckRequest,
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

export async function getDecks(
  page: number = 1,
  pageSize: number = 20,
): Promise<PaginatedResponse<DeckSummaryDto>> {
  const res = await apiFetch(`/api/decks?page=${page}&pageSize=${pageSize}`);
  return res.json();
}

export async function getDeck(id: string): Promise<DeckDetailDto> {
  const res = await apiFetch(`/api/decks/${id}`);
  return res.json();
}

export async function updateDeck(
  id: string,
  data: UpdateDeckRequest,
): Promise<DeckDetailDto> {
  const res = await apiFetch(`/api/decks/${id}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });

  return res.json();
}

export async function deleteDeck(id: string): Promise<void> {
  await apiFetch(`/api/decks/${id}`, {
    method: "DELETE",
  });
}
