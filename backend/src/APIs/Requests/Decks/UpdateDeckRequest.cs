namespace Flashcards.APIs.Requests.Decks {

    public record UpdateDeckRequest(
        int DeckId,
        string Title,
        string? Description,
        List<UpdateCardRequest> Cards
    );

}