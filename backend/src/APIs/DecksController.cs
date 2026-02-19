using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.Decks;
using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Responses;

namespace Flashcards.APIs {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class DecksController : ControllerBase {
        [HttpGet]
        public ActionResult<PaginatedResponse<DeckSummaryDTO>> GetDecks([FromQuery] int page = 1, [FromQuery] int pageSize = 10) {
            return new PaginatedResponse<DeckSummaryDTO>(new List<DeckSummaryDTO>(), page, pageSize, 0, 0);
        }

        [HttpPost]
        public ActionResult<DeckDetailDTO> CreateDeck([FromBody] CreateDeckRequest request) {
            return CreatedAtAction(nameof(GetDeck), new { deckid = Guid.NewGuid() }, new DeckDetailDTO(Guid.NewGuid(), request.Title, request.Description, request.Cards.Select(c => new CardDTO(c.CardId, c.Term, c.Definition, 0)).ToList(), DateTime.UtcNow, DateTime.UtcNow));
        }

        [HttpGet("{deckid}")]
        public ActionResult<DeckDetailDTO> GetDeck(Guid deckid) {
            return new DeckDetailDTO(deckid, "Title", "Description", new List<CardDTO>(), DateTime.UtcNow, DateTime.UtcNow);
        }

        [HttpPut("{deckid}")]
        public ActionResult<DeckDetailDTO> UpdateDeck(Guid deckid, [FromBody] UpdateDeckRequest request) {
            return new DeckDetailDTO(deckid, request.Title, request.Description, request.Cards.Select(c => new CardDTO(c.CardId, c.Term, c.Definition, 0)).ToList(), DateTime.UtcNow, DateTime.UtcNow);
        }

        [HttpDelete("{deckid}")]
        public IActionResult DeleteDeck(Guid deckid) {
            return NoContent();
        }

        [HttpPost("{deckid}/cards")]
        public ActionResult<CardDTO> CreateCard(Guid deckid, [FromBody] CreateCardRequest request) {
            return CreatedAtAction(nameof(GetCard), new { deckid = deckid, cardId = Guid.NewGuid() }, new CardDTO(Guid.NewGuid(), request.Term, request.Definition, 0));
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
