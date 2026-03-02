namespace Flashcards.APIs.Requests.User {

    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );

}