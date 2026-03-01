namespace Flashcards.APIs.Responses {

    public static class ErrorCodes {
        public const string Conflict = "conflict";
        public const string Unauthorized = "unauthorized";
        public const string NotFound = "not_found";
        public const string ValidationError = "validation_error";
        public const string ServerError = "server_error";
    }

    public record ErrorResponse(
        string Error,
        string Message,
        Dictionary<string, string[]>? Details = null
    );

}