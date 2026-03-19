namespace Flashcards.APIs.DTOs.Decks {

    public record ReviewSessionDTO(
        Guid SessionId,
        Guid DeckId,
        DateTime ReviewedAt,
        List<CardDTO> ReviewCards
    );

}
