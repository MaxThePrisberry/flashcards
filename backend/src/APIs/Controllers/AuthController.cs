using Microsoft.AspNetCore.Mvc;
using Flashcards.APIs.Requests.User;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Services.Auth;

namespace Flashcards.APIs.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly AuthService _authService;

        public AuthController(AuthService authService) {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<AuthResponse>> Signup([FromBody] SignupRequest request) {
            var result = await _authService.SignupAsync(request);
            return StatusCode(201, result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request) {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
    }
}
