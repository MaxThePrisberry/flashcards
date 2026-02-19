namespace Flashcards.APIs.Requests.Decks {

    public record CreateDeckRequest(
        Guid DeckId,
        string Title,
        string? Description,
        List<CreateCardRequest> Cards
    );

}