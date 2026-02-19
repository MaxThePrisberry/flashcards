namespace Flashcards.APIs.DTOs.Decks {

    public record DeckSummaryDTO(
        Guid deckId,
        string Title,
        string Description,
        int cardCount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

}