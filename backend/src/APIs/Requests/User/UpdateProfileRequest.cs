namespace Flashcards.APIs.Requests.User {

    public record UpdateProfileRequest(
        string? DisplayName,
        string? Email
    );

}