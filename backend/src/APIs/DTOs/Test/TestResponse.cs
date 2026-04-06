namespace Flashcards.APIs.DTOs.Test {

    public record TestResponse(
        Guid DeckId,
        string DeckTitle,
        int QuestionCount,
        List<TestQuestionDto> Questions
    );

    public record TestQuestionDto(
        Guid CardId,
        string Direction,
        string Prompt,
        List<TestOptionDto> Options,
        int CorrectOptionIndex
    );

    public record TestOptionDto(
        int Index,
        string Text
    );

}
