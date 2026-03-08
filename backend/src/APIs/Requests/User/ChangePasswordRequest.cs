using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.User {

    public record ChangePasswordRequest(
        [Required]
        string CurrentPassword,

        [Required]
        [MinLength(8)]
        string NewPassword
    );

}