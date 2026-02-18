namespace Flashcards.APIs.Requests.User;

public record LoginRequest(
    string Email { get; set; } = string.Empty;
    string Password { get; set; } = string.Empty;
);