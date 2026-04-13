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
  likeCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface DeckDetailDto {
  id: string;
  title: string;
  description: string;
  isPublic: boolean;
  cards: CardDto[];
  createdAt: string;
  updatedAt: string;
}

export interface CardDto {
  id: string;
  term: string;
  definition: string;
  position: number;
  termType: string;
  definitionType: string;
}

export interface CreateDeckRequest {
  title: string;
  description?: string;
  isPublic: boolean;
  cards: CreateCardRequest[];
}

export interface CreateCardRequest {
  term: string;
  definition: string;
  termType?: string;
  definitionType?: string;
}

export interface UpdateDeckRequest {
  title: string;
  description: string;
  isPublic: boolean;
  cards: UpdateCardRequest[];
}

export interface UpdateCardRequest {
  term: string;
  definition: string;
  termType?: string;
  definitionType?: string;
}

// --- Tests ---

export interface TestResponse {
  deckId: string;
  deckTitle: string;
  questionCount: number;
  questions: TestQuestionDto[];
}

export interface TestQuestionDto {
  cardId: string;
  direction: "term_to_definition" | "definition_to_term";
  prompt: string;
  options: TestOptionDto[];
  correctOptionIndex: number;
}

export interface TestOptionDto {
  index: number;
  text: string;
}

// --- Study Sessions ---

export interface SubmitReviewRequest {
  needsReview: string[];
}

export interface ReviewSessionDto {
  sessionId: string;
  deckId: string;
  reviewedAt: string;
  reviewCards: CardDto[];
}

// --- Pagination ---

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
