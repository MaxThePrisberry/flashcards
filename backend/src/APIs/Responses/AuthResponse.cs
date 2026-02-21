using Flashcards.APIs.DTOs.User;
namespace Flashcards.APIs.Responses {

    public record AuthResponse(
        string Token,
        int ExpiresIn,
        UserDTO User
    );

}