using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.Decks {

    public record UpdateDeckRequest(
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
        string Title,

        [Required(AllowEmptyStrings = true, ErrorMessage = "Description is required.")]
        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        string Description,

        [Required(ErrorMessage = "Cards are required.")]
        [MinLength(1, ErrorMessage = "At least one card is required.")]
        [MaxLength(500, ErrorMessage = "A deck cannot have more than 500 cards.")]
        List<CardRequest> Cards
    );

}
