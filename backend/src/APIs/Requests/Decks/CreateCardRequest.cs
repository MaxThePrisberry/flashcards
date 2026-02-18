namespace Flashcards.APIs.Requests.Decks;

public record CreateCardRequest(
    string Term,
    string Definition
);