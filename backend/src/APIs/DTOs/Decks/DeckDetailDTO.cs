namespace Flashcards.APIs.DTOs.Decks {

    public record DeckDetailDTO(
        Guid Id,
        string Title,
        string Description,
        List<CardDTO> Cards,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

}