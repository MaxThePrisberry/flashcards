namespace Flashcards.APIs;

{
    [ApiController]
    [Route("api/[controller]")]
    public class DeckController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetDeck(int id)
        {
            return Ok(new { message = $"Getting deck {id}" });
        }

        [HttpPost]
        public IActionResult CreateDeck([FromBody] CreateDeckRequest request)
        {
            return Ok(new { message = $"Creating deck {request.Name}" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateDeck(int id, [FromBody] UpdateDeckRequest request)
        {
            return Ok(new { message = $"Updating deck {id} with name {request.Name}" });
        }
    }

    public class CreateDeckRequest 
    { 
        public string Name { get; set; } 

        public string Description { get; set; }
    } 
    public class UpdateDeckRequest 
    {
        public string Name { get; set; }
    }
}