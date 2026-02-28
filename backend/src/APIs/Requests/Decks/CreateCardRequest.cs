using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.Decks {

    public record CreateCardRequest(
        [Required(ErrorMessage = "Term is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Term must be between 1 and 500 characters.")]
        string Term,

        [Required(ErrorMessage = "Definition is required.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Definition must be between 1 and 2000 characters.")]
        string Definition
    );

}
