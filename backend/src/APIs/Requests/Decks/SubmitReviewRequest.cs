using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.Decks {

    public record SubmitReviewRequest(
        [Required(ErrorMessage = "NeedsReview is required.")]
        List<Guid> NeedsReview
    );

}
