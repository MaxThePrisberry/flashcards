using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.Decks;
using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Services.Decks;
using System.Security.Claims;

namespace Flashcards.APIs.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DecksController : ControllerBase {
        private readonly DeckService _deckService;

        public DecksController(DeckService deckService) {
            _deckService = deckService;
        }

        [HttpPost]
        public async Task<ActionResult<DeckDetailDTO>> CreateDeck([FromBody] CreateDeckRequest request) {
            if (!TryGetUserId(out var userId)) {
                return Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, "Invalid user ID."));
            }

            var result = await _deckService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetDeck), new { deckid = result.Id }, result);
        }

        [HttpGet("{deckid}")]
        public async Task<ActionResult<DeckDetailDTO>> GetDeck(Guid deckid) {
            if (!TryGetUserId(out var userId)) {
                return Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, "Invalid user ID."));
            }

            var result = await _deckService.GetDeckAsync(deckid, userId);
            return Ok(result);
        }

        [HttpPut("{deckid}")]
        public async Task<ActionResult<DeckDetailDTO>> UpdateDeck(Guid deckid, [FromBody] UpdateDeckRequest request) {
            if (!TryGetUserId(out var userId)) {
                return Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, "Invalid user ID."));
            }

            var result = await _deckService.UpdateAsync(deckid, request, userId);
            return Ok(result);
        }

        private bool TryGetUserId(out Guid userId) {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim != null && Guid.TryParse(claim, out userId)) {
                return true;
            }
            userId = Guid.Empty;
            return false;
        }
    }
}
