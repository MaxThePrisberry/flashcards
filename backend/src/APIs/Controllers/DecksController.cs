using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.Decks;
using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Services.Decks;
using Flashcards.APIs.Services.Reviews;
using Flashcards.APIs.Exceptions;
using System.Security.Claims;

namespace Flashcards.APIs.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DecksController : ControllerBase {
        private readonly DeckService _deckService;
        private readonly ReviewService _reviewService;

        public DecksController(DeckService deckService, ReviewService reviewService) {
            _deckService = deckService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<DeckSummaryDTO>>> GetDecks(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) {
            var userId = GetUserId();
            var result = await _deckService.GetDecksAsync(userId, page, pageSize);
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

        [HttpGet("{id}/reviews/latest")]
        public async Task<ActionResult<ReviewSessionDTO>> GetLatestReview(Guid id) {
            var userId = GetUserId();
            var result = await _reviewService.GetLatestReviewAsync(id, userId);
            return Ok(result);
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
