namespace Flashcards.APIs.DTOs.User {

    public record UserDTO(
        Guid Id,
        string Email,
        string DisplayName,
        DateTime CreatedAt
    );

}