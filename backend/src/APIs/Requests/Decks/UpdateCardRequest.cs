namespace Flashcards.APIs.Requests.Decks {

    public record UpdateCardRequest(
        Guid DeckId,
        Guid CardId,
        string Term,
        string Definition,
        bool Delete = false
    );

}
