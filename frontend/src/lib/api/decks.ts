import type {
  DeckDetailDto,
  DeckSummaryDto,
  CreateDeckRequest,
  UpdateDeckRequest,
  PaginatedResponse,
  SubmitReviewRequest,
  TestResponse,
  ReviewSessionDto,
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
  search?: string,
  signal?: AbortSignal,
): Promise<PaginatedResponse<DeckSummaryDto>> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });

  if (search && search.trim()) {
    params.set("search", search.trim());
  }

  const res = await apiFetch(`/api/decks?${params.toString()}`, {
    signal,
  });

  return res.json();
}

export async function getDeck(
  id: string,
  signal?: AbortSignal,
): Promise<DeckDetailDto> {
  const res = await apiFetch(`/api/decks/${id}`, { signal });
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

export async function generateTest(
  id: string,
  signal?: AbortSignal,
): Promise<TestResponse> {
  const res = await apiFetch(`/api/decks/${id}/test`, {
    method: "POST",
    signal,
  });
  return res.json();
}

export async function submitDeckReview(
  deckId: string,
  data: SubmitReviewRequest,
): Promise<ReviewSessionDto> {
  const res = await apiFetch(`/api/decks/${deckId}/reviews`, {
    method: "POST",
    body: JSON.stringify(data),
  });

  return res.json();
}

export async function getLatestReview(
  id: string,
  signal?: AbortSignal,
): Promise<ReviewSessionDto> {
  const res = await apiFetch(`/api/decks/${id}/reviews/latest`, { signal });
  return res.json();
}

export async function likeDeck(id: string): Promise<void> {
  await apiFetch(`/api/decks/${id}/like`, {
    method: "POST",
  });
}

export async function unlikeDeck(id: string): Promise<void> {
  await apiFetch(`/api/decks/${id}/like`, {
    method: "DELETE",
  });
}

export async function getLikeStatus(
  id: string,
  signal?: AbortSignal,
): Promise<boolean> {
  const res = await apiFetch(`/api/decks/${id}/likes/status`, { signal });
  const data: { isLiked: boolean } = await res.json();
  return data.isLiked;
}
