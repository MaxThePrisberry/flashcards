using Microsoft.EntityFrameworkCore;
using Flashcards.APIs.Entities;
using Flashcards.APIs.Exceptions;

namespace Flashcards.APIs.Services.Likes
{
    public class LikeService
    {
        private readonly AppDbContext _dbContext;

        public LikeService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LikeDeckAsync(Guid deckId, Guid userId)
        {
            // Check if deck exists and is accessible (either owned by user or public)
            var deck = await _dbContext.Decks
                .FirstOrDefaultAsync(d => d.DeckId == deckId && (d.UserId == userId || d.IsPublic));

            if (deck == null)
            {
                throw new NotFoundException("Deck not found.");
            }

            // Check if user already liked this deck
            var existingLike = await _dbContext.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.DeckId == deckId);

            if (existingLike != null)
            {
                // Already liked - this is idempotent, so just return success
                return;
            }

            // Create new like
            var like = new Like
            {
                LikeId = Guid.NewGuid(),
                UserId = userId,
                DeckId = deckId
            };

            _dbContext.Likes.Add(like);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnlikeDeckAsync(Guid deckId, Guid userId)
        {
            var like = await _dbContext.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.DeckId == deckId);

            if (like == null)
            {
                // Not liked - this is idempotent, so just return success
                return;
            }

            _dbContext.Likes.Remove(like);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetLikeCountAsync(Guid deckId)
        {
            return await _dbContext.Likes
                .Where(l => l.DeckId == deckId)
                .CountAsync();
        }

        public async Task<bool> HasUserLikedDeckAsync(Guid deckId, Guid userId)
        {
            return await _dbContext.Likes
                .AnyAsync(l => l.UserId == userId && l.DeckId == deckId);
        }
    }
}
