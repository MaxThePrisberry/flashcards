namespace Flashcards.APIs.DTOs.Decks {

    public record DeckDetailDTO(
        int DeckId,
        string Title,
        string Description,
        List<CardDTO> Cards,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

}