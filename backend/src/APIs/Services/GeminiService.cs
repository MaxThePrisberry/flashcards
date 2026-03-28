using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flashcards.APIs.Exceptions;

namespace Flashcards.APIs.Services.Gemini {

    public class GeminiService : IGeminiService {

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        private static readonly JsonSerializerOptions JsonOptions = new() {
            PropertyNameCaseInsensitive = true
        };

        public GeminiService(HttpClient httpClient, IConfiguration configuration) {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
            _model = configuration["Gemini:Model"] ?? "gemini-3.1-flash-lite-preview";
        }

        public async Task<List<CardDistractors>> GenerateDistractorsAsync(List<CardInfo> cards) {
            if (string.IsNullOrWhiteSpace(_apiKey)) {
                throw new LlmUnavailableException("Gemini API key is not configured. Set the GEMINI_API_KEY environment variable.");
            }

            var prompt = BuildPrompt(cards);

            var requestBody = new {
                contents = new[] {
                    new {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                systemInstruction = new {
                    parts = new[] { new { text = "You are a helpful assistant that generates multiple-choice distractors for flashcard quizzes. You respond with ONLY valid JSON, no markdown formatting." } }
                },
                generationConfig = new {
                    temperature = 0.8,
                    responseMimeType = "application/json"
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            var request = new HttpRequestMessage(HttpMethod.Post, url) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response;
            try {
                response = await _httpClient.SendAsync(request);
            } catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) {
                throw new LlmUnavailableException($"Failed to reach Gemini API: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode) {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new LlmUnavailableException(
                    $"Gemini API returned {(int)response.StatusCode}: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return ParseResponse(responseBody, cards);
        }

        private static string BuildPrompt(List<CardInfo> cards) {
            var sb = new StringBuilder();
            sb.AppendLine("You are generating multiple-choice distractors for a flashcard quiz.");
            sb.AppendLine();
            sb.AppendLine("For each card below, generate exactly 3 WRONG answers for each direction:");
            sb.AppendLine("- 3 fake definitions (for when the term is shown as the question)");
            sb.AppendLine("- 3 fake terms (for when the definition is shown as the question)");
            sb.AppendLine();
            sb.AppendLine("RULES:");
            sb.AppendLine("- Every distractor MUST be provably, factually WRONG — not \"less correct\" or \"debatable\"");
            sb.AppendLine("- Distractors must be plausible enough to test real knowledge");
            sb.AppendLine("- Match the style, length, and domain of the real answers");
            sb.AppendLine("- Distractors MUST NOT match any other term or definition listed below");
            sb.AppendLine("- You may adapt concepts from the deck to create plausible distractors");
            sb.AppendLine();
            sb.AppendLine("Cards:");

            foreach (var card in cards) {
                sb.AppendLine($"{card.Index}. Term: \"{card.Term}\" | Definition: \"{card.Definition}\"");
            }

            sb.AppendLine();
            sb.AppendLine("Respond with ONLY valid JSON in this exact format:");
            sb.AppendLine("{\"cards\":[{\"cardIndex\":1,\"fakeDefinitions\":[\"...\",\"...\",\"...\"],\"fakeTerms\":[\"...\",\"...\",\"...\"]}, ...]}");

            return sb.ToString();
        }

        private static List<CardDistractors> ParseResponse(string responseBody, List<CardInfo> cards) {
            GeminiResponse? geminiResponse;
            try {
                geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseBody, JsonOptions);
            } catch (JsonException ex) {
                throw new LlmUnavailableException($"Failed to parse Gemini response: {ex.Message}");
            }

            var content = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrWhiteSpace(content)) {
                throw new LlmUnavailableException("Gemini returned an empty response.");
            }

            // Parse the inner JSON content
            DistractorResponse? distractorResponse;
            try {
                distractorResponse = JsonSerializer.Deserialize<DistractorResponse>(content, JsonOptions);
            } catch (JsonException ex) {
                throw new LlmUnavailableException($"Failed to parse distractor JSON: {ex.Message}");
            }

            if (distractorResponse?.Cards == null || distractorResponse.Cards.Count == 0) {
                throw new LlmUnavailableException("Gemini returned no distractor data.");
            }

            // Build a set of all real terms and definitions for validation
            var realValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var card in cards) {
                realValues.Add(card.Term.Trim());
                realValues.Add(card.Definition.Trim());
            }

            // Validate and filter distractors
            var result = new List<CardDistractors>();
            foreach (var card in distractorResponse.Cards) {
                var filteredDefs = card.FakeDefinitions?
                    .Where(d => !string.IsNullOrWhiteSpace(d) && !realValues.Contains(d.Trim()))
                    .Select(d => d.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToList() ?? new List<string>();

                var filteredTerms = card.FakeTerms?
                    .Where(t => !string.IsNullOrWhiteSpace(t) && !realValues.Contains(t.Trim()))
                    .Select(t => t.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToList() ?? new List<string>();

                result.Add(new CardDistractors(card.CardIndex, filteredDefs, filteredTerms));
            }

            return result;
        }

        // Internal DTOs for Gemini response parsing
        private class GeminiResponse {
            [JsonPropertyName("candidates")]
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate {
            [JsonPropertyName("content")]
            public ContentBody? Content { get; set; }
        }

        private class ContentBody {
            [JsonPropertyName("parts")]
            public List<Part>? Parts { get; set; }
        }

        private class Part {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }

        private class DistractorResponse {
            [JsonPropertyName("cards")]
            public List<DistractorCard>? Cards { get; set; }
        }

        private class DistractorCard {
            [JsonPropertyName("cardIndex")]
            public int CardIndex { get; set; }

            [JsonPropertyName("fakeDefinitions")]
            public List<string>? FakeDefinitions { get; set; }

            [JsonPropertyName("fakeTerms")]
            public List<string>? FakeTerms { get; set; }
        }
    }
}
