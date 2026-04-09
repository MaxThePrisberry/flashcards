namespace Flashcards.APIs.DTOs.Decks {

    public record DeckSummaryDTO(
        Guid Id,
        string Title,
        string Description,
        int CardCount,
        int LikeCount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

}