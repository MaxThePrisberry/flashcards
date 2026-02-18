namespace Flashcards.APIs.Requests.User;

public record SignupRequest(
    string Email { get; set; } = string.Empty;
    string Password { get; set; } = string.Empty;
    string DisplayName { get; set; } = string.Empty;
);