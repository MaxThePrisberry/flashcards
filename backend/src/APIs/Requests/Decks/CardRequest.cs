using System.ComponentModel.DataAnnotations;

namespace Flashcards.APIs.Requests.Decks {

    public record CardRequest(
        [Required(ErrorMessage = "Term is required.")]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Term is required.")]
        string Term,

        [Required(ErrorMessage = "Definition is required.")]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Definition is required.")]
        string Definition,

        string TermType = "text",
        string DefinitionType = "text"
    );

}
