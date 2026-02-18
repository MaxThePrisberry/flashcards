using Microsoft.AspNetCore.Mvc;

namespace Flashcards.APIs
{
    [ApiController]
    [Route("api/[controller]")]
    public class DecksController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetDeck(Guid id)
        {
            return Ok(new { message = $"Getting deck {id}" });
        }

        [HttpPost]
        public IActionResult CreateDeck([FromBody] CreateDeckRequest request)
        {
            return Ok(new { message = $"Creating deck {request.Name}" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateDeck(Guid id, [FromBody] UpdateDeckRequest request)
        {
            return Ok(new { message = $"Updating deck {id} with name {request.Name}" });
        }
    }

    public class CreateDeckRequest 
    { 
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    } 
    public class UpdateDeckRequest 
    {
        public string Name { get; set; } = string.Empty;
    }
}