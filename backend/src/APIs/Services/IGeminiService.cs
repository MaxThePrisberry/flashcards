namespace Flashcards.APIs.Services.Gemini {

    public interface IGeminiService {
        Task<List<CardDistractors>> GenerateDistractorsAsync(string deckTitle, string deckDescription, List<CardInfo> cards);
    }

    public record CardInfo(int Index, string Term, string Definition);

    public record CardDistractors(int CardIndex, List<string> FakeDefinitions, List<string> FakeTerms);

}
