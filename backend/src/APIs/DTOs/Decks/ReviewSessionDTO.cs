namespace Flashcards.APIs.DTOs.Decks {

    public record ReviewSessionDTO(
        int SessionId,
        Guid DeckId,
        DateTime ReviewedAt,
        List<CardDTO> ReviewCards
    );

}
