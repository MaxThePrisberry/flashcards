using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flashcards.APIs.Requests.User;
using Flashcards.APIs.DTOs.User;
using Flashcards.APIs.Responses;

namespace Flashcards.APIs {
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class authController : ControllerBase {
        [HttpPost("signup")]
        public Task<ActionResult<AuthResponse>> Signup(SignupRequest req) {

            /* 
            Signs up a User
            TODO: 
            - connect to database to store vals, retrieve ID
            - generate real JWT token
            */

            UserDTO user = new UserDTO(Guid.NewGuid(), req.Email, req.DisplayName, DateTime.Now);
            return Task.FromResult<ActionResult<AuthResponse>>(
                Ok(new AuthResponse(
                    "Test Token",
                    3600,
                    user
                ))
            );
        }


        [HttpPost("login")]
        public Task<ActionResult<AuthResponse>> Login(LoginRequest req) {

            /* 
            Logs in a User
            TODO:
            - connect to database to retrieve user details
            - generate real JWT token
            */

            UserDTO user = new UserDTO(Guid.NewGuid(), req.Email, "Test Display name", DateTime.Now);
            return Task.FromResult<ActionResult<AuthResponse>>(
                Ok(new AuthResponse(
                    "Test Token",
                    3600,
                    user
                ))
            );
        }
    }
}