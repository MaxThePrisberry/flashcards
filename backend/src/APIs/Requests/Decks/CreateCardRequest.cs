namespace Flashcards.APIs.Requests.Decks {

    public record CreateCardRequest(
        Guid DeckId,
        Guid CardId,
        string Term,
        string Definition
    );

}