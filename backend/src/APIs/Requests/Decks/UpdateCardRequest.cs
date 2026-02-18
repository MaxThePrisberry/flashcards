namespace Flashcards.APIs.Requests.Decks;

public record UpdateCardRequest(
    string Term,
    string Definition
);
