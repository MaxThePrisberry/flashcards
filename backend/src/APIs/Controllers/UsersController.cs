using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.DTOs.User;
using Flashcards.APIs.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flashcards.APIs.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase {
        private readonly AppDbContext _dbContext;

        public UsersController(AppDbContext dbContext) {
            _dbContext = dbContext;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDTO>> GetMe() {
            var userId = GetUserId();

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) {
                throw new UnauthorizedException("User not found.");
            }

            return Ok(new UserDTO(user.UserId, user.Email, user.Username, user.CreatedAt));
        }

        private Guid GetUserId() {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim != null && Guid.TryParse(claim, out var userId)) {
                return userId;
            }
            throw new UnauthorizedException("Invalid user ID.");
        }
    }
}
