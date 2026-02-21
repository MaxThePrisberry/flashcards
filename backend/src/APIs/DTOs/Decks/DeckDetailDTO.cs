namespace Flashcards.APIs.DTOs.Decks {

    public record DeckDetailDto(
        Guid Id,
        string Title,
        string Description,
        List<CardDto> Cards,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

}