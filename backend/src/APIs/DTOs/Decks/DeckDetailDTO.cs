namespace Flashcards.APIs.DTOs.Decks {

    public record DeckDetailDTO(
        Guid Id,
        Guid OwnerId,
        string Title,
        string Description,
        bool IsPublic,
        List<CardDTO> Cards,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

}
