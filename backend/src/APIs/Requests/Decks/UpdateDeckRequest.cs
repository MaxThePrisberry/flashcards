namespace Flashcards.APIs.Requests.Decks {

    public record UpdateDeckRequest(
        string Title,
        string? Description,
        List<UpdateCardRequest> Cards
    );

}
