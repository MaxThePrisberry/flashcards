namespace Flashcards.APIs.Responses;

public record ErrorResponse(
    string Code,
    string Message,
    Dictionary<string, string[]>? Details = null
);