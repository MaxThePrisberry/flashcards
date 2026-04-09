using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.Decks;
using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Services.Decks;
using Flashcards.APIs.Services.Reviews;
using Flashcards.APIs.Services.Tests;
using Flashcards.APIs.DTOs.Test;
using Flashcards.APIs.Services.Likes;
using Flashcards.APIs.Exceptions;
using System.Security.Claims;

namespace Flashcards.APIs.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DecksController : ControllerBase {
        private readonly DeckService _deckService;
        private readonly ReviewService _reviewService;
        private readonly TestService _testService;
        private readonly LikeService _likeService;

        public DecksController(DeckService deckService, ReviewService reviewService, TestService testService, LikeService likeService) {
            _deckService = deckService;
            _reviewService = reviewService;
            _testService = testService;
            _likeService = likeService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<DeckSummaryDTO>>> GetDecks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null) {
            var userId = GetUserId();
            var result = await _deckService.GetDecksAsync(userId, page, pageSize, search);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<DeckDetailDTO>> CreateDeck([FromBody] CreateDeckRequest request) {
            var userId = GetUserId();
            var result = await _deckService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetDeck), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeckDetailDTO>> GetDeck(Guid id) {
            var userId = GetUserId();
            var result = await _deckService.GetDeckAsync(id, userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeckDetailDTO>> UpdateDeck(Guid id, [FromBody] UpdateDeckRequest request) {
            var userId = GetUserId();
            var result = await _deckService.UpdateAsync(id, request, userId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeck(Guid id) {
            var userId = GetUserId();
            await _deckService.DeleteAsync(id, userId);
            return NoContent();
        }

        [HttpPost("{id}/reviews")]
        public async Task<ActionResult<ReviewSessionDTO>> SubmitReview(Guid id, [FromBody] SubmitReviewRequest request) {
            var userId = GetUserId();
            var result = await _reviewService.SubmitReviewAsync(id, userId, request.NeedsReview);
            return CreatedAtAction(nameof(GetLatestReview), new { id }, result);
        }

        [HttpPost("{id}/test")]
        public async Task<ActionResult<TestResponse>> GenerateTest(Guid id) {
            var userId = GetUserId();
            var result = await _testService.GenerateTestAsync(id, userId);
            return Ok(result);
        }

        [HttpGet("{id}/reviews/latest")]
        public async Task<ActionResult<ReviewSessionDTO>> GetLatestReview(Guid id) {
            var userId = GetUserId();
            var result = await _reviewService.GetLatestReviewAsync(id, userId);
            return Ok(result);
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeDeck(Guid id) {
            var userId = GetUserId();
            await _likeService.LikeDeckAsync(id, userId);
            return NoContent();
        }

        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikeDeck(Guid id) {
            var userId = GetUserId();
            await _likeService.UnlikeDeckAsync(id, userId);
            return NoContent();
        }

        [HttpGet("{id}/likes/count")]
        public async Task<ActionResult<int>> GetLikeCount(Guid id) {
            var count = await _likeService.GetLikeCountAsync(id);
            return Ok(new { count });
        }

        [HttpGet("{id}/likes/status")]
        public async Task<ActionResult<bool>> GetLikeStatus(Guid id) {
            var userId = GetUserId();
            var isLiked = await _likeService.HasUserLikedDeckAsync(id, userId);
            return Ok(new { isLiked });
        }

        private Guid GetUserId() {
            // TEMPORARY: Return dummy user ID for testing without auth
            // return Guid.Parse("99999999-9999-9999-9999-999999999999");

            // REAL IMPLEMENTATION:
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim != null && Guid.TryParse(claim, out var userId)) {
                return userId;
            }
            throw new UnauthorizedException("Invalid user ID.");
        }
    }
}
