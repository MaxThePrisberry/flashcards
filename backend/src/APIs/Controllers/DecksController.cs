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

        ── Deck Endpoints ────────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<DeckDetailDTO>> CreateDeck([FromBody] CreateDeckRequest request) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId)) {
                return Unauthorized(new { message = "Invalid user ID" });
            }

            var result = await _deckService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetDeck), new { deckid = result.Id }, result);

            // MOCK VERSION (for testing without database):
            // var mockDeckId = Guid.NewGuid();
            // var mockCards = request.Cards.Select((card, index) => new CardDTO(
            //     Guid.NewGuid(),
            //     card.Term,
            //     card.Definition,
            //     index
            // )).ToList();
            //
            // var mockDeck = new DeckDetailDTO(
            //     mockDeckId,
            //     request.Title,
            //     request.Description ?? "",
            //     mockCards,
            //     DateTime.UtcNow,
            //     DateTime.UtcNow
            // );
            //
            // return Ok(mockDeck);
        }

        [HttpGet("{deckid}")]
        public ActionResult<DeckDetailDTO> GetDeck(Guid deckid) {
            return new DeckDetailDTO(deckid, "Title", "Description", new List<CardDTO>(), DateTime.UtcNow, DateTime.UtcNow);
        }


        // [HttpGet]
        // public ActionResult<PaginatedResponse<DeckSummaryDTO>> GetDecks(
        //     [FromQuery] int page = 1,
        //     [FromQuery] int pageSize = 10
        // ) {
        //     return new PaginatedResponse<DeckSummaryDTO>(
        //         new List<DeckSummaryDTO>(),
        //         page,
        //         pageSize,
        //         0,
        //         0
        //     );
        // }

        // [HttpPut("{deckid}")]
        // public ActionResult<DeckDetailDTO> UpdateDeck(Guid deckid, [FromBody] UpdateDeckRequest request) {
        //     var cards = request.Cards
        //         .Select(c => new CardDTO(c.CardId, c.Term, c.Definition, 0))
        //         .ToList();

        //     return new DeckDetailDTO(deckid, request.Title, request.Description, cards, DateTime.UtcNow, DateTime.UtcNow);
        // }

    }
}
