export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  expiresIn: number;
  user: UserDto;
}

export interface CardDto {
  id: string;
  term: string;
  definition: string;
}

export interface DeckDto {
  id: string;
  title: string;
  description: string;
  cardCount: number;
  cards?: CardDto[];
  createdAt: string;
}

export interface DeckListResponse {
  items: DeckDto[];
}
