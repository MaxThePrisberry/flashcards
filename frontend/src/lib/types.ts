// --- Error ---

export interface ErrorResponse {
  error: string;
  message: string;
  details?: Record<string, string[]>;
}

// --- Auth ---

export interface AuthResponse {
  token: string;
  expiresIn: number;
  user: UserDto;
}

// --- Users ---

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  createdAt: string;
}

// --- Decks ---

export interface DeckSummaryDto {
  id: string;
  title: string;
  description: string;
  cardCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface DeckDetailDto {
  id: string;
  title: string;
  description: string;
  cards: CardDto[];
  createdAt: string;
  updatedAt: string;
}

export interface CardDto {
  id: string;
  term: string;
  definition: string;
  position: number;
}

export interface CreateDeckRequest {
  title: string;
  description?: string;
  cards: CreateCardRequest[];
}

export interface CreateCardRequest {
  term: string;
  definition: string;
}

export interface UpdateDeckRequest {
  title: string;
  description: string;
  cards: UpdateCardRequest[];
}

export interface UpdateCardRequest {
  term: string;
  definition: string;
}

// --- Pagination ---

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
