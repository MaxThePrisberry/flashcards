using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.User;
using Flashcards.APIs.DTOs.User;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Services.Auth;


namespace Flashcards.APIs {
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class AuthController : ControllerBase {
        private readonly AuthService _authService;

        public AuthController(AuthService authService) {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<AuthResponse>> Signup([FromBody] SignupRequest request) {
            try {
                var result = await _authService.SignupAsync(request);
                return Ok(result);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }

            // MOCK VERSION (for testing without database):
            // var mockUser = new UserDTO(
            //     Guid.NewGuid(),
            //     request.Email,
            //     request.DisplayName,
            //     DateTime.UtcNow
            // );
            // var mockResponse = new AuthResponse(
            //     "mock-jwt-token-for-testing",
            //     3600,
            //     mockUser
            // );
            // return Ok(mockResponse);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request) {
            try {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }

            // MOCK VERSION (for testing without database):
            // var mockUser = new UserDTO(
            //     Guid.NewGuid(),
            //     request.Email,
            //     "Mock User",
            //     DateTime.UtcNow
            // );
            // var mockResponse = new AuthResponse(
            //     "mock-jwt-token-for-testing",
            //     3600,
            //     mockUser
            // );
            // return Ok(mockResponse);
        }
    }
}