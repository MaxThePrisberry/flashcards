using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Requests.Decks;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.APIs.Services.Decks {
    public class DeckService {
        private readonly AppDbContext _dbContext;

        public DeckService(AppDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<DeckDetailDTO> CreateAsync(CreateDeckRequest request, Guid userId) {
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
            var cardDtos = pairs.Select((pair, i) => new CardDTO(
                pair.PairId,
                request.Cards[i].Term,
                request.Cards[i].Definition,
                i
            )).ToList();

            return new DeckDetailDTO(
                deck.DeckId,
                deck.Title,
                deck.Description,
                cardDtos,
                deck.CreatedAt,
                deck.UpdatedAt
            );
        }

        public async Task<DeckDetailDTO> UpdateAsync(UpdateDeckRequest request, Guid userId) {
            // Load deck with existing pairs and items
            var deck = await _dbContext.Decks
                .Include(d => d.Pairs)
                    .ThenInclude(p => p.Item1)
                .Include(d => d.Pairs)
                    .ThenInclude(p => p.Item2)
                .FirstOrDefaultAsync(d => d.DeckId == request.DeckId);

            if (deck == null) {
                throw new Exception("Deck not found");
            }

            if (deck.UserId != userId) {
                throw new Exception("Unauthorized");
            }

            // Update deck metadata
            deck.Title = request.Title;
            deck.Description = request.Description;
            deck.UpdatedAt = DateTime.UtcNow;

            // Remove all existing pairs and items
            foreach (var pair in deck.Pairs) {
                _dbContext.Items.Remove(pair.Item1);
                _dbContext.Items.Remove(pair.Item2);
            }
            _dbContext.Pairs.RemoveRange(deck.Pairs);
            await _dbContext.SaveChangesAsync();

            // Look up CardTypes
            var termType = await _dbContext.Types.FirstAsync(t => t.TypeName == "term");
            var defType = await _dbContext.Types.FirstAsync(t => t.TypeName == "definition");

            // Build all items first, filtering out cards marked for deletion
            var termItems = new List<Item>();
            var defItems = new List<Item>();

            for (int i = 0; i < request.Cards.Count; i++) {
                var cardRequest = request.Cards[i];

                // Skip cards marked for deletion
                if (cardRequest.Delete) {
                    continue;
                }

                var termItem = new Item {
                    DeckId = deck.DeckId,
                    TypeId = termType.TypeId,
                    Value = cardRequest.Term,
                    Position = i
                };

                var defItem = new Item {
                    DeckId = deck.DeckId,
                    TypeId = defType.TypeId,
                    Value = cardRequest.Definition,
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

            for (int i = 0; i < termItems.Count; i++) {
                var pair = new Pair {
                    DeckId = deck.DeckId,
                    Item1Id = termItems[i].ItemId,
                    Item2Id = defItems[i].ItemId,
                    Position = i
                };

                _dbContext.Pairs.Add(pair);
                pairs.Add(pair);
            }
            await _dbContext.SaveChangesAsync(); // one trip for all pairs

            // Build card DTOs from the saved pairs
            var cardDtos = new List<CardDTO>();
            for (int i = 0; i < pairs.Count; i++) {
                cardDtos.Add(new CardDTO(
                    pairs[i].PairId,
                    termItems[i].Value,
                    defItems[i].Value,
                    i
                ));
            }

            return new DeckDetailDTO(
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
