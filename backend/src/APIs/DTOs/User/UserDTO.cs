namespace Flashcards.APIs.DTOs.User {

    public record UserDTO(
        Guid userId,
        string email,
        string displayName,
        DateTime createdAt
    );

}