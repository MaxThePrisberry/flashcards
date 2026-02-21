using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.User;
using Flashcards.APIs.DTOs.User;
using Flashcards.APIs.Responses;

namespace Flashcards.APIs {
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class usersController : ControllerBase {
        [HttpGet("me")]
        public ActionResult<UserDTO> GetUser() {

            /* 
            Gets a User
            TODO:
            - connect to database to retrieve user details
            */


            return Ok(new UserDTO(
                userId: Guid.NewGuid(),
                email: "user@example.com",
                displayName: "Test User",
                createdAt: DateTime.UtcNow
            ));
        }

        [HttpPatch("me")]
        public ActionResult<UserDTO> UpdateProfile(UpdateProfileRequest req) {

            /* 
            Updates a User
            TODO:
            - connect to database to retrieve user details to return
            */


            return Ok(new UserDTO(
                userId: Guid.NewGuid(),
                email: req.Email ?? "user@example.com",
                displayName: req.DisplayName ?? "Test User",
                createdAt: DateTime.UtcNow
            ));
        }


        [HttpPut("me/password")]
        public ActionResult<UserDTO> UpdateProfile(ChangePasswordRequest req) {

            /* 
            Updates a User
            TODO:
            - connect to database to retrieve user details to return
            */


            return Ok(204);
        }


    }
}