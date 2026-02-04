using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Authentication.Services;
using SmartStorage_Shared.DTO;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Controllers
{
    [ApiVersion($"{Utils.apiVersion}")]
    [Route("api/storage/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IUserAuthService _userAuthService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ILoginService loginService,
            IUserAuthService userAuthService,
            ILogger<AuthController> logger)
        {
            _loginService = loginService;
            _userAuthService = userAuthService;
            _logger = logger;
        }

        [HttpGet("user-by-username")]
        public IActionResult GetUser([FromQuery] string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                _logger.LogWarning("Get User failed: Missing username");

                return BadRequest("Username is required.");
            }
            var user = _userAuthService.FindByUsername(userName);

            if (user == null) return BadRequest("Usuário não existe.");

            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAllUser()
        {
            return Ok(_userAuthService.FindAllUsers());
        }

        [HttpGet("{userId}")]
        public IActionResult GetUserById(int userId)
        {
            try
            {
                if (userId.Equals(0))
                    throw new Exception("O campo ID do Usuário é obrigatório.");

                return Ok(_userAuthService.FindUserById(userId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("signin")]
        [AllowAnonymous]
        public IActionResult SignIn([FromBody] UserDTO user)
        {
            _logger.LogInformation("Attempting to sign in user: {username}", user.Username);

            if (user == null ||
                string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                _logger.LogWarning("Sign in failed: Missing username or password");

                return BadRequest("Username and password are required.");
            }
            var token = _loginService.ValidateCredentials(user);

            if (token == null) return Unauthorized();

            _logger.LogInformation("User {username} signed in successfully", user.Username);

            return Ok(token);
        }

        [HttpPost("update-credentials")]
        public IActionResult UpdateUserCredentials([FromBody] User user)
        {
            _logger.LogInformation("Attempting to update user: {username}", user.Username);

            if (user == null ||
                string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Password) ||
                string.IsNullOrWhiteSpace(user.FullName))
            {
                _logger.LogWarning("Update failed: Missing username, password or full name");

                return BadRequest("Username, password and full name are required.");
            }

            _loginService.UpdateCredentials(user);

            _logger.LogInformation("User {username} updated successfully", user.Username);

            return Ok(user);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public IActionResult Refresh(
            [FromBody] TokenDTO tokenDto)
        {
            if (tokenDto == null) return BadRequest("Invalid client request!");

            var token = _loginService.ValidateCredentials(tokenDto);

            if (token == null) return Unauthorized();

            return Ok(token);
        }

        [HttpPost("revoke")]
        public IActionResult Revoke()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Invalid Client Request!");

            var result = _loginService.RevokeToken(username);

            if (!result) return BadRequest("Invalid Client Request!");

            return NoContent();
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] AccountCredentialsDTO user)
        {
            if (user == null)
                return BadRequest("Invalid client request!");

            var result = _loginService.Create(user);

            return Ok(result);
        }

    }
}
