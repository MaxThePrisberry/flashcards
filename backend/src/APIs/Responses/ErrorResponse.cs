namespace Flashcards.APIs.Responses {

    public record ErrorResponse(
        string Error,
        string Message,
        Dictionary<string, string[]>? Details = null
    );

}