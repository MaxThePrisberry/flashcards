using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.User {

    public record SignupRequest(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        string Email,

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        string Password,

        [Required(ErrorMessage = "Display name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Display name must be between 1 and 100 characters.")]
        string DisplayName
    );

}
