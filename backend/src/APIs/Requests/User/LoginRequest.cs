namespace Flashcards.APIs.Requests.User {

    public record LoginRequest(
        string Email,
        string Password
    );

}