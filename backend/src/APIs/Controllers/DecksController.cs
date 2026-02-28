using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.Decks;
using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Services.Decks;
using Flashcards.APIs.Exceptions;
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId)) {
                return Unauthorized(new ErrorResponse("unauthorized", "Invalid user ID."));
            }

            var result = await _deckService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetDeck), new { deckid = result.Id }, result);
        }

        [HttpGet("{deckid}")]
        public async Task<ActionResult<DeckDetailDTO>> GetDeck(Guid deckid) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId)) {
                return Unauthorized(new ErrorResponse("unauthorized", "Invalid user ID."));
            }

            try {
                var result = await _deckService.GetDeckAsync(deckid, userId);
                return Ok(result);
            } catch (NotFoundException ex) {
                return NotFound(new ErrorResponse("not_found", ex.Message));
            }
        }
    }
}
