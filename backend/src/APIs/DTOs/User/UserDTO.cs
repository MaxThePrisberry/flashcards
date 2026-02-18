namespace Flashcards.APIs.DTOs.User;

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt
);