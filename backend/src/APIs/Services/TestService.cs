using Flashcards.APIs.DTOs.Test;
using Flashcards.APIs.Entities;
using Flashcards.APIs.Exceptions;
using Flashcards.APIs.Services.Gemini;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.APIs.Services.Tests {

    public class TestService {

        private readonly AppDbContext _dbContext;
        private readonly IGeminiService _geminiService;
        // Random.Shared is thread-safe in .NET 6+
        private static Random Rng => Random.Shared;

        public TestService(AppDbContext dbContext, IGeminiService geminiService) {
            _dbContext = dbContext;
            _geminiService = geminiService;
        }

        public async Task<TestResponse> GenerateTestAsync(Guid deckId, Guid userId) {
            // Verify deck ownership
            var deck = await _dbContext.Decks
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeckId == deckId && d.UserId == userId);

            if (deck == null) {
                throw new NotFoundException("Deck not found.");
            }

            // Fetch all pairs with their items
            var pairs = await _dbContext.Pairs
                .AsNoTracking()
                .Where(p => p.DeckId == deckId)
                .Include(p => p.Item1)
                .Include(p => p.Item2)
                .OrderBy(p => p.Position)
                .ToListAsync();

            if (pairs.Count == 0) {
                throw new ValidationException("Deck has no cards to generate a test from.");
            }

            // Exclude cards with image content — LLM distractors don't work with base64 images
            pairs = pairs
                .Where(p => p.Item1.TypeId != CardType.ImageTypeId && p.Item2.TypeId != CardType.ImageTypeId)
                .ToList();

            if (pairs.Count == 0) {
                throw new ValidationException("No text-based cards available to generate a test from.");
            }

            // Check distractor cache (expire after 24 hours)
            var pairIds = pairs.Select(p => p.PairId).ToList();
            var cacheExpiry = DateTime.UtcNow.AddHours(-24);
            var cachedDistractors = await _dbContext.Distractors
                .AsNoTracking()
                .Where(d => pairIds.Contains(d.PairId) && d.CreatedAt > cacheExpiry)
                .ToListAsync();

            var distractorsByPair = cachedDistractors
                .GroupBy(d => d.PairId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Identify pairs missing distractors (need 6 per pair: 3 per direction)
            var uncachedPairs = pairs
                .Where(p => !distractorsByPair.TryGetValue(p.PairId, out var ds)
                            || ds.Count(d => d.Direction == 0) < 3
                            || ds.Count(d => d.Direction == 1) < 3)
                .ToList();

            if (uncachedPairs.Count > 0) {
                await GenerateAndCacheDistractors(deck.Title, deck.Description, uncachedPairs, distractorsByPair);
            }

            // Build test questions
            var questions = BuildQuestions(pairs, distractorsByPair);

            return new TestResponse(
                deckId,
                deck.Title,
                questions.Count,
                questions
            );
        }

        private async Task GenerateAndCacheDistractors(
            string deckTitle, string deckDescription,
            List<Pair> uncachedPairs,
            Dictionary<Guid, List<Distractor>> distractorsByPair) {

            var cardInfos = uncachedPairs.Select((p, i) => new CardInfo(
                i + 1,
                p.Item1.Value,
                p.Item2.Value
            )).ToList();

            var generated = await _geminiService.GenerateDistractorsAsync(deckTitle, deckDescription, cardInfos);

            // Map card index back to pair — the LLM may return cards in any order
            var indexToPair = new Dictionary<int, Pair>();
            for (int i = 0; i < uncachedPairs.Count; i++) {
                indexToPair[i + 1] = uncachedPairs[i];
            }

            var newDistractors = new List<Distractor>();

            foreach (var cardDistractors in generated) {
                if (!indexToPair.TryGetValue(cardDistractors.CardIndex, out var pair))
                    continue;

                var pairDistractors = new List<Distractor>();

                foreach (var fakeDef in cardDistractors.FakeDefinitions.Take(3)) {
                    var d = new Distractor {
                        DistractorId = Guid.NewGuid(),
                        PairId = pair.PairId,
                        Direction = 0,
                        Value = fakeDef,
                        CreatedAt = DateTime.UtcNow
                    };
                    newDistractors.Add(d);
                    pairDistractors.Add(d);
                }

                foreach (var fakeTerm in cardDistractors.FakeTerms.Take(3)) {
                    var d = new Distractor {
                        DistractorId = Guid.NewGuid(),
                        PairId = pair.PairId,
                        Direction = 1,
                        Value = fakeTerm,
                        CreatedAt = DateTime.UtcNow
                    };
                    newDistractors.Add(d);
                    pairDistractors.Add(d);
                }

                distractorsByPair[pair.PairId] = pairDistractors;
            }

            if (newDistractors.Count > 0) {
                try {
                    _dbContext.Distractors.AddRange(newDistractors);
                    await _dbContext.SaveChangesAsync();
                } catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505") {
                    // Unique constraint violation from concurrent generation — safe to ignore.
                    // The distractors are already in distractorsByPair for building the test.
                    _dbContext.ChangeTracker.Clear();
                }
            }
        }

        private static List<TestQuestionDto> BuildQuestions(
            List<Pair> pairs,
            Dictionary<Guid, List<Distractor>> distractorsByPair) {

            var questions = new List<TestQuestionDto>(pairs.Count);

            foreach (var pair in pairs) {
                // Randomly pick direction: 0 = term→definition, 1 = definition→term
                var direction = Rng.Next(2);
                var directionStr = direction == 0 ? "term_to_definition" : "definition_to_term";

                string prompt;
                string correctAnswer;
                int distractorDirection;

                if (direction == 0) {
                    // Show term, pick definition
                    prompt = pair.Item1.Value;
                    correctAnswer = pair.Item2.Value;
                    distractorDirection = 0; // fake definitions
                } else {
                    // Show definition, pick term
                    prompt = pair.Item2.Value;
                    correctAnswer = pair.Item1.Value;
                    distractorDirection = 1; // fake terms
                }

                // Get distractors for this direction
                var distractors = distractorsByPair.TryGetValue(pair.PairId, out var ds)
                    ? ds.Where(d => d.Direction == distractorDirection).Select(d => d.Value).ToList()
                    : new List<string>();

                // Build options: correct answer + up to 3 distractors
                var options = new List<string> { correctAnswer };
                options.AddRange(distractors.Take(3));

                // Shuffle options
                Shuffle(options);

                // Find correct answer index after shuffle
                var correctIndex = options.IndexOf(correctAnswer);

                var optionDtos = options.Select((text, idx) => new TestOptionDto(idx, text)).ToList();

                questions.Add(new TestQuestionDto(
                    pair.PairId,
                    directionStr,
                    prompt,
                    optionDtos,
                    correctIndex
                ));
            }

            // Shuffle question order
            Shuffle(questions);

            return questions;
        }

        private static void Shuffle<T>(List<T> list) {
            for (int i = list.Count - 1; i > 0; i--) {
                int j = Rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
