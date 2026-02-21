namespace Flashcards.APIs.Requests.Decks {

    public record CreateDeckRequest(
        string Title,
        string? Description,
        List<CreateCardRequest> Cards
    );

}