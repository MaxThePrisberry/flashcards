using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.Decks;
using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Responses;

namespace Flashcards.APIs.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class DecksController : ControllerBase {

        // ── Deck Endpoints ────────────────────────────────────────────────

        [HttpGet]
        public ActionResult<PaginatedResponse<DeckSummaryDTO>> GetDecks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        ) {
            return new PaginatedResponse<DeckSummaryDTO>(
                new List<DeckSummaryDTO>(),
                page,
                pageSize,
                0,
                0
            );
        }

        [HttpPost]
        public async Task<ActionResult<DeckDetailDTO>> CreateDeck([FromBody] CreateDeckRequest request) {
            var userID = //JWT
            var deck = 
        }

        [HttpGet("{deckid}")]
        public ActionResult<DeckDetailDTO> GetDeck(Guid deckid) {
            return new DeckDetailDTO(deckid, "Title", "Description", new List<CardDTO>(), DateTime.UtcNow, DateTime.UtcNow);
        }

        [HttpPut("{deckid}")]
        public ActionResult<DeckDetailDTO> UpdateDeck(Guid deckid, [FromBody] UpdateDeckRequest request) {
            var cards = request.Cards
                .Select(c => new CardDTO(c.CardId, c.Term, c.Definition, 0))
                .ToList();

            return new DeckDetailDTO(deckid, request.Title, request.Description, cards, DateTime.UtcNow, DateTime.UtcNow);
        }

        [HttpDelete("{deckid}")]
        public IActionResult DeleteDeck(Guid deckid) {
            return NoContent();
        }

        // ── Card Endpoints ────────────────────────────────────────────────

        [HttpPost("{deckid}/cards")]
        public ActionResult<CardDTO> CreateCard(Guid deckid, [FromBody] CreateCardRequest request) {
            var newCardId = Guid.NewGuid();

            return CreatedAtAction(nameof(GetCard), new { deckid = deckid, cardId = newCardId },
                new CardDTO(newCardId, request.Term, request.Definition, 0)
            );
        }

        [HttpGet("{deckid}/cards/{cardId}")]
        public ActionResult<CardDTO> GetCard(Guid deckid, Guid cardId) {
            return new CardDTO(cardId, "Term", "Definition", 0);
        }

        [HttpPut("{deckid}/cards/{cardId}")]
        public ActionResult<CardDTO> UpdateCard(Guid deckid, Guid cardId, [FromBody] UpdateCardRequest request) {
            return new CardDTO(cardId, request.Term, request.Definition, 0);
        }

        [HttpDelete("{deckid}/cards/{cardId}")]
        public IActionResult DeleteCard(Guid deckid, Guid cardId) {
            return NoContent();
        }
    }
}
