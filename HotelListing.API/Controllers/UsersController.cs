using HotelListing.API.Contracts;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAuthManager _authManager;

        public UsersController(IAuthManager authManager)
        {
            this._authManager = authManager;
        }

        // POST: api/Users/register
        [HttpPost]
        // This "register" route is appended to the controller's route to form the full route, in this case base route is api/Users
        // So, the full route is api/Users + register = api/Users/register
        [Route("register")]

        /*
         ProducesResponseType Attributes: These attributes are used to explicitly document the types 
         of responses that an API action can return. They are especially useful for OpenAPI (Swagger) documentation, 
         helping client developers understand the possible outcomes of calling the API.
         */
        // Indicates that this action method can return a 400 Bad Request response.
        // This is typically returned when the input payload doesn't meet the expected format or validation rules.
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // Indicates a possible 500 Internal Server Error response.
        // This can happen if there's an unexpected failure during the processing of the request, such as a database error.
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // Indicates that a 200 OK response is possible, signifying a successful registration.
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] ApiUserDTO userPayload)
        {
            var errors = await _authManager.Register(userPayload);

            if (errors.Any())
            {
                // We fill the ModelState with the errors because the validation errors
                // that are automatically handled by ASP.NET Core usually populate the ModelState.
                // This allows us to return a structured response to the client containing all validation errors.
                foreach (var error in errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return BadRequest(ModelState); // Return a 400 Bad Request with the validation errors
            }

            return Ok(); // Return a 200 OK if registration is successful
        }

        // POST: api/Users/login
        [HttpPost]
        // This "login" route is appended to the controller's route to form the full route, in this case base route is api/Users
        // So, the full route is api/Users + login = api/Users/login
        [Route("login")]

        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginPayload)
        {
            var authResponse = await _authManager.Login(loginPayload);

            if (authResponse == null)
            {
                return Unauthorized(); // Return a 401 Unauthorized if login is unsuccessful
            }

            return Ok(authResponse); // Return a 200 OK if login is successful
        }

        // POST: api/Users/refreshtoken
        [HttpPost]
        // This "refreshtoken" route is appended to the controller's route to form the full route, in this case base route is api/Users
        // So, the full route is api/Users + refreshtoken = api/Users/refreshtoken
        [Route("refreshtoken")]

        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] AuthResponseDTO request)
        {
            var authResponse = await _authManager.VerifyRefreshToken(request);

            if (authResponse == null)
            {
                return Unauthorized(); // Return a 401 Unauthorized if login is unsuccessful
            }

            return Ok(authResponse); // Return a 200 OK if login is successful
        }
    }
}
