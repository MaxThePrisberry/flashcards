namespace Flashcards.APIs.Responses;

public record AuthResponse(
    string Token,
    int ExpiresIn,
    UserDto User
);