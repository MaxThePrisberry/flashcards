using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.User;
using Flashcards.APIs.DTOs.User;
using Flashcards.APIs.Responses;

namespace Flashcards.APIs {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class UserController : ControllerBase {
        [HttpGet]

        public ActionResult<UserDTO> GetUser() {
            return new UserDTO(
                userId: Guid.NewGuid(),
                email: ""
                displayName: "",
                createdAt: DateTime.UtcNow
            )
    }