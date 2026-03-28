namespace Flashcards.APIs.Services.OpenAi {

    public interface IOpenAiService {
        Task<List<CardDistractors>> GenerateDistractorsAsync(List<CardInfo> cards);
    }

    public record CardInfo(int Index, string Term, string Definition);

    public record CardDistractors(int CardIndex, List<string> FakeDefinitions, List<string> FakeTerms);

}
