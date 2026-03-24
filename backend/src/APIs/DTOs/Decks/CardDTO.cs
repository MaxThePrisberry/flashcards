namespace Flashcards.APIs.DTOs.Decks {

    public record CardDTO(
        Guid Id,
        string Term,
        string Definition,
        int Position,
        string TermType = "text",
        string DefinitionType = "text"
    );

}