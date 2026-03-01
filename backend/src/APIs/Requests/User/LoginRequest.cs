using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.User {

    public record LoginRequest(
        [Required(ErrorMessage = "Email is required.")]
        string Email,

        [Required(ErrorMessage = "Password is required.")]
        string Password
    );

}
