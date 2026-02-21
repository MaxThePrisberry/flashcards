using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.Decks;
using Flashcards.APIs.DTOs.Decks;
using Flashcards.APIs.Responses;
using System.Security.Claims;

namespace Flashcards.APIs.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]

    public class DecksController : ControllerBase {

        // ── Deck Endpoints ────────────────────────────────────────────────

        [HttpGet]
        public ActionResult<PaginatedResponse<DeckSummaryDto>> GetDecks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        ) {
            return new PaginatedResponse<DeckSummaryDto>(
                new List<DeckSummaryDto>(),
                page,
                pageSize,
                0,
                0
            );
        }

        [HttpPost]
        public async Task<ActionResult<DeckDetailDto>> CreateDeck([FromBody] CreateDeckRequest request) {

            /* 
            Creates a Deck
            TODO:
            - connect to database to input deck
            */

            List<CreateCardRequest> cards = request.Cards;
            List<CardDto> returnCards = new List<CardDto>();
            int position = 0;
            foreach (var card in cards) {
                Guid id = Guid.NewGuid();
                string term = card.Term;
                string definition = card.Definition;
                CardDto dto = new CardDto(id, term, definition, position);
                returnCards.Add(dto);
            }

            DeckDetailDto deckDetail = new DeckDetailDto(Guid.NewGuid(), request.Title, request.Description, returnCards, DateTime.Now, DateTime.Now);
            return Ok(deckDetail);
        }

        [HttpGet("{deckid}")]
        public ActionResult<DeckDetailDto> GetDeck(Guid deckid) {
            return new DeckDetailDto(deckid, "Title", "Description", new List<CardDto>(), DateTime.UtcNow, DateTime.UtcNow);
        }

        [HttpPut("{deckid}")]
        public ActionResult<DeckDetailDto> UpdateDeck(Guid deckid, [FromBody] UpdateDeckRequest request) {
            var cards = request.Cards
                .Select(c => new CardDto(Guid.NewGuid(), c.Term, c.Definition, 0))
                .ToList();
            return new DeckDetailDto(deckid, request.Title, request.Description, cards, DateTime.UtcNow, DateTime.UtcNow);
        }

    }
}
