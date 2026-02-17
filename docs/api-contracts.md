# API Contract — Technical Design Document

| | |
|---|---|
| **Project** | Flashcards |
| **Version** | 1.0 |
| **Date** | 2026-02-17 |
| **Wiki** | [Project Wiki](https://github.com/cs428TAs/w2026/wiki/Flashcards) |

---

## Table of Contents

1. [Conventions](#1-conventions)
2. [Error Response Format](#2-error-response-format)
3. [Authentication](#3-authentication)
4. [Users](#4-users)
5. [Decks](#5-decks)
6. [HTTP Status Code Conventions](#6-http-status-code-conventions)
7. [Appendix: TypeScript Interfaces & C# Records](#7-appendix-typescript-interfaces--c-records)

---

## 1. Conventions

- **IDs** — All entity IDs are UUIDs (GUIDs), serialized as lowercase hyphenated strings (e.g. `"3fa85f64-5717-4562-b3fc-2c963f66afa6"`).
- **Timestamps** — All timestamps are ISO 8601 UTC strings (e.g. `"2026-02-17T12:00:00Z"`).
- **Authentication** — JWT Bearer token via the `Authorization` header: `Authorization: Bearer <token>`. All endpoints except `/api/auth/*` require this header.
- **Content-Type** — All request and response bodies are `application/json`.
- **Base URL** — `http://localhost:5000` (configured as `NEXT_PUBLIC_API_URL` in `docker-compose.yml`).

---

## 2. Error Response Format

All error responses from the API share a single shape. This provides a consistent, predictable structure that the frontend can handle with one error-parsing path regardless of which endpoint failed.

### ErrorResponse

```json
{
  "error": "validation_error",
  "message": "One or more fields are invalid.",
  "details": {
    "email": ["Must be a valid email address."],
    "password": ["Must be at least 8 characters."]
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `error` | `string` | Yes | Machine-readable error code for programmatic handling (e.g. `"validation_error"`, `"not_found"`, `"unauthorized"`, `"conflict"`). |
| `message` | `string` | Yes | Human-readable message suitable for display to the user. |
| `details` | `Record<string, string[]>` | No | Per-field validation errors. Keys are field names, values are arrays of error messages. Only present for `validation_error`. |

**Design Rationale.** Separating `error` (machine-readable) from `message` (human-readable) lets the frontend switch on error codes for control flow while still having a displayable string. The optional `details` map enables inline field-level validation feedback in forms without inventing a separate response shape for validation errors.

---

## 3. Authentication

### POST /api/auth/signup

Create a new user account and receive a JWT.

**Request — SignupRequest**

```json
{
  "email": "jane@example.com",
  "password": "s3cureP@ss",
  "displayName": "Jane"
}
```

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `email` | `string` | Yes | Valid email format; unique across all users | Login identifier. |
| `password` | `string` | Yes | Min 8 characters | Account password. |
| `displayName` | `string` | Yes | 1–100 characters | Public-facing display name. |

**Response — AuthResponse** `201 Created`

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "jane@example.com",
    "displayName": "Jane",
    "createdAt": "2026-02-17T12:00:00Z"
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `token` | `string` | Signed JWT for subsequent authenticated requests. |
| `expiresIn` | `integer` | Token lifetime in seconds. Lets the frontend schedule refresh without decoding the JWT. |
| `user` | `UserDto` | The newly created user profile (see [UserDto](#get-apiusersme)). |

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 400 | `validation_error` | Missing/invalid fields |
| 409 | `conflict` | Email already registered |

**Design Rationale.** Email is used as the login identifier rather than a username to avoid "username already taken" friction — everyone has a unique email. `displayName` is separate because users want a public-facing name that isn't their email. There is no "confirm password" field; that is a frontend UI concern, not an API concern. The response bundles the JWT token AND user data together, eliminating a second `GET /api/users/me` roundtrip after signup.

---

### POST /api/auth/login

Authenticate with existing credentials.

**Request — LoginRequest**

```json
{
  "email": "jane@example.com",
  "password": "s3cureP@ss"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `email` | `string` | Yes | Account email. |
| `password` | `string` | Yes | Account password. |

**Response — AuthResponse** `200 OK`

Same shape as [signup response](#post-apiauthsignup).

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "jane@example.com",
    "displayName": "Jane",
    "createdAt": "2026-02-17T12:00:00Z"
  }
}
```

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 400 | `validation_error` | Missing/invalid fields |
| 401 | `unauthorized` | Invalid email or password |

**Design Rationale.** A simple credential pair, consistent with signup using email as the identifier. The response reuses `AuthResponse` so the frontend login and signup flows converge to the same post-auth state: store the token and hydrate the user.

---

## 4. Users

All endpoints in this section require authentication.

### GET /api/users/me

Return the authenticated user's profile.

**Response — UserDto** `200 OK`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "jane@example.com",
  "displayName": "Jane",
  "createdAt": "2026-02-17T12:00:00Z"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `id` | `string (uuid)` | Unique user identifier. |
| `email` | `string` | User's email address. |
| `displayName` | `string` | Public display name. |
| `createdAt` | `string (datetime)` | Account creation timestamp (ISO 8601 UTC). |

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 401 | `unauthorized` | Missing or invalid JWT |

**Design Rationale.** `UserDto` is the safe public representation of a user — no password hash ever leaves the server. The UUID `id` is included for client-side reference and future features like deck-sharing attribution. `createdAt` supports "member since" display on profile pages.

---

### PATCH /api/users/me

Update the authenticated user's profile. Uses PATCH semantics: only include the fields you want to change.

**Request — UpdateProfileRequest**

```json
{
  "displayName": "Jane Doe"
}
```

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `displayName` | `string` | No | 1–100 characters | New display name. |
| `email` | `string` | No | Valid email format; unique | New email address. May trigger re-verification in a future iteration. |

At least one field must be provided.

**Response — UserDto** `200 OK`

Returns the full updated user profile (same shape as [GET /api/users/me](#get-apiusersme)).

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 400 | `validation_error` | Invalid fields or empty request body |
| 401 | `unauthorized` | Missing or invalid JWT |
| 409 | `conflict` | New email already in use |

**Design Rationale.** PATCH semantics let the frontend send only what changed, avoiding accidental overwrites. Email and display name changes live together because they are both "profile info" — but password changes are deliberately separated (see below) because they are a distinct security action.

---

### PUT /api/users/me/password

Change the authenticated user's password.

**Request — ChangePasswordRequest**

```json
{
  "currentPassword": "s3cureP@ss",
  "newPassword": "n3wS3cure!"
}
```

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `currentPassword` | `string` | Yes | — | Current password for re-authentication. |
| `newPassword` | `string` | Yes | Min 8 characters | The new password. |

**Response** `204 No Content`

No response body.

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 400 | `validation_error` | New password doesn't meet requirements |
| 401 | `unauthorized` | Missing/invalid JWT or wrong current password |

**Design Rationale.** Password changes are separated from profile updates because they are a security action requiring re-authentication via the current password. Returning 204 (no body) is appropriate since there is no meaningful data to return — the password hash is never exposed.

---

## 5. Decks

All endpoints in this section require authentication. Users can only access their own decks.

There are no separate card endpoints. Cards are components of a deck, not independently addressable resources. They are always created, read, updated, and deleted as part of their parent deck. This matches the real UX: users edit a deck as a whole document (add/remove/reorder cards, then save).

---

### GET /api/decks

List the authenticated user's decks with pagination.

**Query Parameters**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | `integer` | `1` | Page number (1-indexed). |
| `pageSize` | `integer` | `20` | Items per page (max 100). |

**Response — PaginatedResponse\<DeckSummaryDto\>** `200 OK`

```json
{
  "items": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "title": "Spanish Vocabulary",
      "description": "Common Spanish words and phrases",
      "cardCount": 42,
      "createdAt": "2026-02-10T08:30:00Z",
      "updatedAt": "2026-02-17T14:15:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

#### PaginatedResponse\<T\>

| Field | Type | Description |
|-------|------|-------------|
| `items` | `T[]` | Array of results for the current page. |
| `page` | `integer` | Current page number. |
| `pageSize` | `integer` | Number of items per page. |
| `totalCount` | `integer` | Total number of items across all pages. |
| `totalPages` | `integer` | Total number of pages. |

#### DeckSummaryDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | `string (uuid)` | Unique deck identifier. |
| `title` | `string` | Deck title. |
| `description` | `string` | Deck description (empty string if none). |
| `cardCount` | `integer` | Number of cards in the deck. |
| `createdAt` | `string (datetime)` | When the deck was created (ISO 8601 UTC). |
| `updatedAt` | `string (datetime)` | When the deck was last modified (ISO 8601 UTC). |

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 401 | `unauthorized` | Missing or invalid JWT |

**Design Rationale.** `DeckSummaryDto` is used in list/grid views and sends `cardCount` (an integer) instead of the full card array. This keeps list payloads small: 50 decks times a few fields, rather than 50 decks times N cards each. Pagination via `PaginatedResponse<T>` prevents unbounded payloads as users accumulate decks. Standard offset-based pagination is sufficient for the expected dataset sizes.

---

### POST /api/decks

Create a new deck with its cards.

**Request — CreateDeckRequest**

```json
{
  "title": "Spanish Vocabulary",
  "description": "Common Spanish words and phrases",
  "cards": [
    { "term": "Hola", "definition": "Hello" },
    { "term": "Gracias", "definition": "Thank you" }
  ]
}
```

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `title` | `string` | Yes | 1–200 characters | Deck title. |
| `description` | `string` | No | Max 1000 characters | Deck description. Defaults to empty string. |
| `cards` | `CreateCardRequest[]` | Yes | Min 1 card | Initial set of cards. |

#### CreateCardRequest (inline)

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `term` | `string` | Yes | 1–500 characters | Card front / term. |
| `definition` | `string` | Yes | 1–2000 characters | Card back / definition. |

**Response — DeckDetailDto** `201 Created`

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "title": "Spanish Vocabulary",
  "description": "Common Spanish words and phrases",
  "cards": [
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "term": "Hola",
      "definition": "Hello",
      "position": 0
    },
    {
      "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
      "term": "Gracias",
      "definition": "Thank you",
      "position": 1
    }
  ],
  "createdAt": "2026-02-17T12:00:00Z",
  "updatedAt": "2026-02-17T12:00:00Z"
}
```

#### DeckDetailDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | `string (uuid)` | Unique deck identifier. |
| `title` | `string` | Deck title. |
| `description` | `string` | Deck description (empty string if none). |
| `cards` | `CardDto[]` | All cards in the deck, ordered by `position`. |
| `createdAt` | `string (datetime)` | When the deck was created (ISO 8601 UTC). |
| `updatedAt` | `string (datetime)` | When the deck was last modified (ISO 8601 UTC). |

#### CardDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | `string (uuid)` | Unique card identifier. |
| `term` | `string` | Card front / term. |
| `definition` | `string` | Card back / definition. |
| `position` | `integer` | Zero-based display order within the deck. |

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 400 | `validation_error` | Missing/invalid fields, or empty cards array |
| 401 | `unauthorized` | Missing or invalid JWT |

**Design Rationale.** Cards are provided inline at creation time because creating an empty deck then adding cards one-by-one is bad UX — users always have at least one card in mind when they create a deck. The request omits `id` and `position` for cards because the server assigns UUIDs and auto-positions by array index. `description` is optional because many users skip it.

---

### GET /api/decks/{id}

Get a single deck with all its cards.

**Path Parameters**

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `string (uuid)` | Deck ID. |

**Response — DeckDetailDto** `200 OK`

Same shape as [POST /api/decks response](#deckdetaildto).

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 401 | `unauthorized` | Missing or invalid JWT |
| 404 | `not_found` | Deck does not exist or belongs to another user |

**Design Rationale.** Returns the full deck with embedded cards because cards are ALWAYS needed when viewing a single deck — there is no scenario where you fetch a deck but don't want its cards. Returning 404 for another user's deck (rather than 403) prevents user enumeration.

---

### PUT /api/decks/{id}

Replace an entire deck and its cards.

**Path Parameters**

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `string (uuid)` | Deck ID. |

**Request — UpdateDeckRequest**

```json
{
  "title": "Spanish Vocabulary (Updated)",
  "description": "Common Spanish words and phrases — expanded",
  "cards": [
    { "term": "Hola", "definition": "Hello" },
    { "term": "Adiós", "definition": "Goodbye" },
    { "term": "Por favor", "definition": "Please" }
  ]
}
```

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `title` | `string` | Yes | 1–200 characters | Deck title. |
| `description` | `string` | Yes | Max 1000 characters | Deck description. Send empty string to clear. |
| `cards` | `UpdateCardRequest[]` | Yes | Min 1 card | Complete set of cards. Replaces all existing cards. |

#### UpdateCardRequest (inline)

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `term` | `string` | Yes | 1–500 characters | Card front / term. |
| `definition` | `string` | Yes | 1–2000 characters | Card back / definition. |

**Response — DeckDetailDto** `200 OK`

Same shape as [POST /api/decks response](#deckdetaildto).

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 400 | `validation_error` | Missing/invalid fields, or empty cards array |
| 401 | `unauthorized` | Missing or invalid JWT |
| 404 | `not_found` | Deck does not exist or belongs to another user |

**Design Rationale.** This uses full replacement (PUT) rather than partial update (PATCH). The frontend holds the complete deck in state while the user edits — adding, removing, reordering cards — then saves the whole document atomically. The server replaces all cards with the provided array: deleted cards are those absent from the new array, added cards are new entries. This eliminates all partial-state and consistency issues that come with per-card PATCH operations. Deck editing is a "save document" pattern, not a "tweak one field" pattern.

---

### DELETE /api/decks/{id}

Delete a deck and all its cards.

**Path Parameters**

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `string (uuid)` | Deck ID. |

**Response** `204 No Content`

No response body.

**Error Responses**

| Status | Error Code | When |
|--------|------------|------|
| 401 | `unauthorized` | Missing or invalid JWT |
| 404 | `not_found` | Deck does not exist or belongs to another user |

---

## 6. HTTP Status Code Conventions

| Status Code | Meaning | Used For |
|-------------|---------|----------|
| `200 OK` | Success | GET, PATCH, PUT responses that return data |
| `201 Created` | Resource created | POST signup, POST create deck |
| `204 No Content` | Success, no body | DELETE, password change |
| `400 Bad Request` | Validation failure | Invalid or missing request fields |
| `401 Unauthorized` | Auth failure | Missing/invalid JWT, wrong password |
| `404 Not Found` | Resource not found | Deck not found or not owned by user |
| `409 Conflict` | Uniqueness violation | Email already registered |
| `500 Internal Server Error` | Server error | Unhandled exceptions (returns `ErrorResponse` with `error: "server_error"`) |

---

## 7. Appendix: TypeScript Interfaces & C# Records

Platform-specific type definitions that map directly to the JSON shapes above, provided for copy-paste convenience.

### TypeScript

```typescript
// --- Error ---

interface ErrorResponse {
  error: string;
  message: string;
  details?: Record<string, string[]>;
}

// --- Auth ---

interface SignupRequest {
  email: string;
  password: string;
  displayName: string;
}

interface LoginRequest {
  email: string;
  password: string;
}

interface AuthResponse {
  token: string;
  expiresIn: number;
  user: UserDto;
}

// --- Users ---

interface UserDto {
  id: string;
  email: string;
  displayName: string;
  createdAt: string;
}

interface UpdateProfileRequest {
  displayName?: string;
  email?: string;
}

interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

// --- Decks ---

interface DeckSummaryDto {
  id: string;
  title: string;
  description: string;
  cardCount: number;
  createdAt: string;
  updatedAt: string;
}

interface DeckDetailDto {
  id: string;
  title: string;
  description: string;
  cards: CardDto[];
  createdAt: string;
  updatedAt: string;
}

interface CardDto {
  id: string;
  term: string;
  definition: string;
  position: number;
}

interface CreateDeckRequest {
  title: string;
  description?: string;
  cards: CreateCardRequest[];
}

interface CreateCardRequest {
  term: string;
  definition: string;
}

interface UpdateDeckRequest {
  title: string;
  description: string;
  cards: UpdateCardRequest[];
}

interface UpdateCardRequest {
  term: string;
  definition: string;
}

// --- Pagination ---

interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
```

### C#

```csharp
// --- Error ---

public record ErrorResponse(
    string Error,
    string Message,
    Dictionary<string, string[]>? Details = null
);

// --- Auth ---

public record SignupRequest(
    string Email,
    string Password,
    string DisplayName
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    int ExpiresIn,
    UserDto User
);

// --- Users ---

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt
);

public record UpdateProfileRequest(
    string? DisplayName,
    string? Email
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

// --- Decks ---

public record DeckSummaryDto(
    Guid Id,
    string Title,
    string Description,
    int CardCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record DeckDetailDto(
    Guid Id,
    string Title,
    string Description,
    List<CardDto> Cards,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CardDto(
    Guid Id,
    string Term,
    string Definition,
    int Position
);

public record CreateDeckRequest(
    string Title,
    string? Description,
    List<CreateCardRequest> Cards
);

public record CreateCardRequest(
    string Term,
    string Definition
);

public record UpdateDeckRequest(
    string Title,
    string Description,
    List<UpdateCardRequest> Cards
);

public record UpdateCardRequest(
    string Term,
    string Definition
);

// --- Pagination ---

public record PaginatedResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
```

---

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| UUIDs over auto-increment IDs | Prevents enumeration attacks; no sequential guessing of resource IDs. |
| Deck-centric (no card endpoints) | Cards are always part of a deck. Matches "save document" UX; atomic updates; simpler API surface. |
| PUT for deck updates (full replace) | No partial state bugs. The frontend always has the full deck in memory during editing. |
| AuthResponse bundles token + user | Saves a roundtrip after login/signup — frontend gets everything it needs in one response. |
| DeckSummaryDto vs DeckDetailDto | List views stay lightweight with `cardCount`; detail views include the full card array. |
| Separate ChangePasswordRequest | Password changes are a security action requiring current-password re-authentication, not a profile edit. |
| `position` field on cards | Preserves user-defined card ordering, which is critical for study flows. |
| 404 instead of 403 for other users' decks | Prevents resource enumeration — attacker can't distinguish "exists but not mine" from "doesn't exist". |
