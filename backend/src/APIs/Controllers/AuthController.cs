using Microsoft.AspNetCore.Mvc;
using Flashcards.APIs.Requests.User;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Services.Auth;
using Flashcards.APIs.Exceptions;

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
            try {
                var result = await _authService.SignupAsync(request);
                return StatusCode(201, result);
            } catch (ConflictException ex) {
                return Conflict(new ErrorResponse("conflict", ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request) {
            try {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            } catch (UnauthorizedException ex) {
                return Unauthorized(new ErrorResponse("unauthorized", ex.Message));
            }
        }
    }
}
