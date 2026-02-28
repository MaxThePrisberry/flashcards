using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.Decks {

    public record CreateDeckRequest(
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
        string Title,

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        string? Description,

        [Required(ErrorMessage = "Cards are required.")]
        [MinLength(1, ErrorMessage = "At least one card is required.")]
        List<CreateCardRequest> Cards
    );

}
