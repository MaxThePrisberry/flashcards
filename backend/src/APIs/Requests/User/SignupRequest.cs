namespace Flashcards.APIs.Requests.User {

    public record SignupRequest(
        string Email,
        string Password,
        string DisplayName
    );

}