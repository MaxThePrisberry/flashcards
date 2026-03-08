using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.User {

    public record UpdateProfileRequest(
        [StringLength(100, MinimumLength = 1)]
        string? DisplayName,

        [EmailAddress]
        string? Email
    );

}