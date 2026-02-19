using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Requests.Decks;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.APIs.Services.Deck {
    public class CreateDeckService {
        private readonly AppDbContext _dbContext;

        public CreateDeckService(AppDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<DeckDetailDto> ExecuteAsync(CreateDeckRequest request, Guid userId) {
            // Create and save the deck
            var deck = new Deck {
                UserId = userId,
                Title = request.Title,
                Description = request.Description
            };

            _dbContext.Decks.Add(deck);
            await _dbContext.SaveChangesAsync();

            // Look up CardTypes for term and definition
            var termType = await _dbContext.Types.FirstAsync(t => t.TypeName == "term");
            var defType  = await _dbContext.Types.FirstAsync(t => t.TypeName == "definition");

            // Build all items first and store them so we can reference their IDs for pairs
            var termItems = new List<Item>();
            var defItems  = new List<Item>();

            for (int i = 0; i < request.Cards.Count; i++) {
                var termItem = new Item {
                    DeckId   = deck.DeckId,
                    TypeId   = termType.TypeId,
                    Value    = request.Cards[i].Term,
                    Position = i
                };

                var defItem = new Item {
                    DeckId   = deck.DeckId,
                    TypeId   = defType.TypeId,
                    Value    = request.Cards[i].Definition,
                    Position = i
                };

                _dbContext.Items.Add(termItem);
                _dbContext.Items.Add(defItem);
                termItems.Add(termItem);
                defItems.Add(defItem);
            }
            await _dbContext.SaveChangesAsync(); // one trip for all items

            // Now build all pairs using the saved item IDs
            var pairs = new List<Pair>();

            for (int i = 0; i < request.Cards.Count; i++) {
                var pair = new Pair {
                    DeckId   = deck.DeckId,
                    Item1Id  = termItems[i].ItemId,
                    Item2Id  = defItems[i].ItemId,
                    Position = i
                };

                _dbContext.Pairs.Add(pair);
                pairs.Add(pair);
            }
            await _dbContext.SaveChangesAsync(); // one trip for all pairs

            // Build card DTOs from the saved pairs
            var cardDtos = pairs.Select((pair, i) => new CardDto(
                pair.PairId,
                request.Cards[i].Term,
                request.Cards[i].Definition,
                i
            )).ToList();

            return new DeckDetailDto(
                deck.DeckId,
                deck.Title,
                deck.Description,
                cardDtos,
                deck.CreatedAt,
                deck.UpdatedAt
            );
        }
    }
}
