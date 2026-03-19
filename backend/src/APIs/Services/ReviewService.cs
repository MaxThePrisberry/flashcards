using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Entities;
using Flashcards.APIs.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.APIs.Services.Reviews {
    public class ReviewService {

        private readonly AppDbContext _dbContext;

        public ReviewService(AppDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<ReviewSessionDTO> SubmitReviewAsync(Guid deckId, Guid userId, List<Guid> needsReview) {
            // Verify deck ownership
            var deck = await _dbContext.Decks
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeckId == deckId && d.UserId == userId);

            if (deck == null) {
                throw new NotFoundException("Deck not found.");
            }

            // Validate all card IDs belong to this deck
            var deckCardIds = await _dbContext.Pairs
                .AsNoTracking()
                .Where(p => p.DeckId == deckId)
                .Select(p => p.PairId)
                .ToListAsync();

            var invalidCardIds = needsReview.Except(deckCardIds).ToList();
            if (invalidCardIds.Any()) {
                throw new ValidationException("One or more card IDs do not belong to this deck.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Create ReviewHistory record
            var reviewHistory = new ReviewHistory {
                HistoryId = Guid.NewGuid(),
                DeckId = deckId,
                UserId = userId,
                ReviewedAt = DateTime.UtcNow
            };

            _dbContext.ReviewHistories.Add(reviewHistory);
            await _dbContext.SaveChangesAsync();

            // Create ReviewPair records for each card that needs review
            var reviewPairs = needsReview.Select(cardId => new ReviewPair {
                ReviewPairId = Guid.NewGuid(),
                HistoryId = reviewHistory.HistoryId,
                PairId = cardId
            }).ToList();

            if (reviewPairs.Any()) {
                _dbContext.ReviewPairs.AddRange(reviewPairs);
                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Get card details for cards that need review
            var reviewCards = await GetCardsForReviewAsync(deckId, needsReview);

            return new ReviewSessionDTO(
                reviewHistory.HistoryId,
                deckId,
                reviewHistory.ReviewedAt,
                reviewCards
            );
        }

        public async Task<ReviewSessionDTO> GetLatestReviewAsync(Guid deckId, Guid userId) {
            // Verify deck ownership
            var deck = await _dbContext.Decks
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeckId == deckId && d.UserId == userId);

            if (deck == null) {
                throw new NotFoundException("Deck not found.");
            }

            // Get the most recent review session for this deck
            var latestReview = await _dbContext.ReviewHistories
                .AsNoTracking()
                .Where(rh => rh.DeckId == deckId && rh.UserId == userId)
                .OrderByDescending(rh => rh.ReviewedAt)
                .Include(rh => rh.ReviewPairs)
                .FirstOrDefaultAsync();

            if (latestReview == null) {
                throw new NotFoundException("No review sessions found for this deck.");
            }

            // Get card IDs that need review
            var cardIdsNeedingReview = latestReview.ReviewPairs
                .Select(rp => rp.PairId)
                .ToList();

            // Get card details for cards that need review
            var reviewCards = await GetCardsForReviewAsync(deckId, cardIdsNeedingReview);

            return new ReviewSessionDTO(
                latestReview.HistoryId,
                deckId,
                latestReview.ReviewedAt,
                reviewCards
            );
        }

        private async Task<List<CardDTO>> GetCardsForReviewAsync(Guid deckId, List<Guid> cardIds) {
            if (!cardIds.Any()) {
                return new List<CardDTO>();
            }

            var cards = await _dbContext.Pairs
                .AsNoTracking()
                .Where(p => p.DeckId == deckId && cardIds.Contains(p.PairId))
                .Include(p => p.Item1)
                .Include(p => p.Item2)
                .OrderBy(p => p.Position)
                .Select(p => new CardDTO(
                    p.PairId,
                    p.Item1.Value,
                    p.Item2.Value,
                    p.Position
                ))
                .ToListAsync();

            return cards;
        }
    }
}
