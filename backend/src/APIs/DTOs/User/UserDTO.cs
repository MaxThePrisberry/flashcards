namespace Flashcards.APIs.DTOs.User {

    public record UserDTO(
        Guid UserId,
        string Email,
        string DisplayName,
        DateTime CreatedAt
    );

}