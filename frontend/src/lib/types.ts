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
