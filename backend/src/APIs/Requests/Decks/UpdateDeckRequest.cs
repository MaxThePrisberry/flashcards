namespace Flashcards.APIs.Requests.Decks {

    public record UpdateDeckRequest(
        Guid DeckId,
        string Title,
        string? Description,
        List<UpdateCardRequest> Cards
    );

}