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
                DeckId = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                Description = request.Description ?? ""
            };

            var (items, pairs, cardDtos) = BuildCardEntities(deck.DeckId, request.Cards);

            _dbContext.Decks.Add(deck);
            _dbContext.Items.AddRange(items);
            _dbContext.Pairs.AddRange(pairs);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return ToDeckDetailDTO(deck, cardDtos);
        }

        public async Task<DeckDetailDTO> GetDeckAsync(Guid deckId, Guid userId) {
            var deck = await _dbContext.Decks
                .Include(d => d.Pairs.OrderBy(p => p.Position))
                    .ThenInclude(p => p.Item1)
                .Include(d => d.Pairs)
                    .ThenInclude(p => p.Item2)
                .FirstOrDefaultAsync(d => d.DeckId == deckId && d.UserId == userId);

            if (deck == null) {
                throw new NotFoundException("Deck not found.");
            }

            var cardDtos = deck.Pairs.Select(p => new CardDTO(
                p.PairId,
                p.Item1.Value,
                p.Item2.Value,
                p.Position
            )).ToList();

            return ToDeckDetailDTO(deck, cardDtos);
        }

        public async Task<DeckDetailDTO> UpdateAsync(Guid deckId, UpdateDeckRequest request, Guid userId) {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var deck = await _dbContext.Decks
                .FirstOrDefaultAsync(d => d.DeckId == deckId && d.UserId == userId);

            if (deck == null) {
                throw new NotFoundException("Deck not found.");
            }

            deck.Title = request.Title;
            deck.Description = request.Description;
            deck.UpdatedAt = DateTime.UtcNow;

            await _dbContext.Pairs.Where(p => p.DeckId == deckId).ExecuteDeleteAsync();
            await _dbContext.Items.Where(i => i.DeckId == deckId).ExecuteDeleteAsync();

            var (items, pairs, cardDtos) = BuildCardEntities(deck.DeckId, request.Cards);

            _dbContext.Items.AddRange(items);
            _dbContext.Pairs.AddRange(pairs);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return ToDeckDetailDTO(deck, cardDtos);
        }

        private static (List<Item> items, List<Pair> pairs, List<CardDTO> cardDtos) BuildCardEntities(
            Guid deckId, IList<CardRequest> cards) {

            var items = new List<Item>(cards.Count * 2);
            var pairs = new List<Pair>(cards.Count);
            var cardDtos = new List<CardDTO>(cards.Count);

            for (int i = 0; i < cards.Count; i++) {
                var termItem = new Item {
                    ItemId = Guid.NewGuid(),
                    DeckId = deckId,
                    TypeId = CardType.TextTypeId,
                    Value = cards[i].Term,
                    Position = i
                };

                var defItem = new Item {
                    ItemId = Guid.NewGuid(),
                    DeckId = deckId,
                    TypeId = CardType.TextTypeId,
                    Value = cards[i].Definition,
                    Position = i
                };

                var pairId = Guid.NewGuid();
                var pair = new Pair {
                    PairId = pairId,
                    DeckId = deckId,
                    Item1Id = termItem.ItemId,
                    Item2Id = defItem.ItemId,
                    Position = i
                };

                items.Add(termItem);
                items.Add(defItem);
                pairs.Add(pair);
                cardDtos.Add(new CardDTO(pairId, cards[i].Term, cards[i].Definition, i));
            }

            return (items, pairs, cardDtos);
        }

        private static DeckDetailDTO ToDeckDetailDTO(Deck deck, List<CardDTO> cards) {
            return new DeckDetailDTO(
                deck.DeckId,
                deck.Title,
                deck.Description,
                cards,
                deck.CreatedAt,
                deck.UpdatedAt
            );
        }
    }
}
