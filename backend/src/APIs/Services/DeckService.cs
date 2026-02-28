using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Requests.Decks;
using Flashcards.APIs.Exceptions;
using Microsoft.EntityFrameworkCore;
using Flashcards.APIs.Entities;

namespace Flashcards.APIs.Services.Decks {
    public class DeckService {
        private readonly AppDbContext _dbContext;

        public DeckService(AppDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<DeckDetailDTO> CreateAsync(CreateDeckRequest request, Guid userId) {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var deck = new Deck {
                UserId = userId,
                Title = request.Title,
                Description = request.Description ?? ""
            };

            _dbContext.Decks.Add(deck);
            await _dbContext.SaveChangesAsync();

            var textType = await _dbContext.Types.FirstOrDefaultAsync(t => t.TypeName == "text")
                ?? throw new InvalidOperationException("Required CardType 'text' not found. Ensure the database is seeded.");

            var termItems = new List<Item>();
            var defItems  = new List<Item>();

            for (int i = 0; i < request.Cards.Count; i++) {
                var termItem = new Item {
                    DeckId   = deck.DeckId,
                    TypeId   = textType.TypeId,
                    Value    = request.Cards[i].Term,
                    Position = i
                };

                var defItem = new Item {
                    DeckId   = deck.DeckId,
                    TypeId   = textType.TypeId,
                    Value    = request.Cards[i].Definition,
                    Position = i
                };

                _dbContext.Items.Add(termItem);
                _dbContext.Items.Add(defItem);
                termItems.Add(termItem);
                defItems.Add(defItem);
            }
            await _dbContext.SaveChangesAsync();

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
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            var cardDtos = pairs.Select((pair, i) => new CardDTO(
                pair.PairId,
                request.Cards[i].Term,
                request.Cards[i].Definition,
                i
            )).ToList();

            return new DeckDetailDTO(
                deck.DeckId,
                deck.Title,
                deck.Description ?? "",
                cardDtos,
                deck.CreatedAt,
                deck.UpdatedAt
            );
        }

        public async Task<DeckDetailDTO> GetDeckAsync(Guid deckId, Guid userId) {
            var deck = await _dbContext.Decks
                .Include(d => d.Pairs.OrderBy(p => p.Position))
                    .ThenInclude(p => p.Item1)
                .Include(d => d.Pairs)
                    .ThenInclude(p => p.Item2)
                .FirstOrDefaultAsync(d => d.DeckId == deckId);

            if (deck == null || deck.UserId != userId) {
                throw new NotFoundException("Deck not found.");
            }

            var cardDtos = deck.Pairs.Select(p => new CardDTO(
                p.PairId,
                p.Item1.Value,
                p.Item2.Value,
                p.Position
            )).ToList();

            return new DeckDetailDTO(
                deck.DeckId,
                deck.Title,
                deck.Description ?? "",
                cardDtos,
                deck.CreatedAt,
                deck.UpdatedAt
            );
        }

        public async Task<DeckDetailDTO> UpdateAsync(Guid deckId, UpdateDeckRequest request, Guid userId) {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var deck = await _dbContext.Decks
                .Include(d => d.Pairs)
                    .ThenInclude(p => p.Item1)
                .Include(d => d.Pairs)
                    .ThenInclude(p => p.Item2)
                .FirstOrDefaultAsync(d => d.DeckId == deckId);

            if (deck == null) {
                throw new NotFoundException("Deck not found.");
            }

            if (deck.UserId != userId) {
                throw new NotFoundException("Deck not found.");
            }

            deck.Title = request.Title;
            deck.Description = request.Description;
            deck.UpdatedAt = DateTime.UtcNow;

            foreach (var pair in deck.Pairs) {
                _dbContext.Items.Remove(pair.Item1);
                _dbContext.Items.Remove(pair.Item2);
            }
            _dbContext.Pairs.RemoveRange(deck.Pairs);
            await _dbContext.SaveChangesAsync();

            var textType = await _dbContext.Types.FirstOrDefaultAsync(t => t.TypeName == "text")
                ?? throw new InvalidOperationException("Required CardType 'text' not found. Ensure the database is seeded.");

            var termItems = new List<Item>();
            var defItems = new List<Item>();

            for (int i = 0; i < request.Cards.Count; i++) {
                var termItem = new Item {
                    DeckId = deck.DeckId,
                    TypeId = textType.TypeId,
                    Value = request.Cards[i].Term,
                    Position = i
                };

                var defItem = new Item {
                    DeckId = deck.DeckId,
                    TypeId = textType.TypeId,
                    Value = request.Cards[i].Definition,
                    Position = i
                };

                _dbContext.Items.Add(termItem);
                _dbContext.Items.Add(defItem);
                termItems.Add(termItem);
                defItems.Add(defItem);
            }
            await _dbContext.SaveChangesAsync();

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
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

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
                deck.Description ?? "",
                cardDtos,
                deck.CreatedAt,
                deck.UpdatedAt
            );
        }
    }
}
