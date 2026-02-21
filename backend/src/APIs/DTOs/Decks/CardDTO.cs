namespace Flashcards.APIs.DTOs.Decks {

    public record CardDto(
        Guid Id,
        string Term,
        string Definition,
        int Position
    );

}