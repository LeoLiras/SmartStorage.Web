using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage.AuthenticationAPI.Services;
using SmartStorage.Shared.Auth;
using SmartStorage.Shared.VO;
using SmartStorage.Shared.Validation;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Controllers
{
    [ApiVersion($"{Utils.Utils.apiVersion}")]
    [Route("api/[controller]/v{version:apiVersion}")]
    [ApiController]
    [Authorize]
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

        [HttpGet("my-account")]
        public IActionResult GetMyAccount()
        {
            var user = _userAuthService.FindByUsername(User.Identity?.Name);

            if (user == null) throw new BusinessException("Usuário não existe.");

            return Ok(UserAccountVO.Parse(user));
        }

        [HttpPut("my-account")]
        public IActionResult UpdateMyAccount([FromBody] MyAccountUpdateVO data)
        {
            var user = _userAuthService.UpdateMyAccount(User.Identity?.Name, data);

            return Ok(UserAccountVO.Parse(user));
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public IActionResult GetAllUser()
        {
            return Ok(UserAccountVO.ParseList(_userAuthService.FindAllUsers()));
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = Role.Admin)]
        public IActionResult GetUserById(int userId)
        {
            if (userId.Equals(0))
                throw new BusinessException("O campo ID do Usuário é obrigatório.");

            return Ok(UserAccountVO.Parse(_userAuthService.FindUserById(userId)));
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = Role.Admin)]
        public IActionResult DeleteUser(int userId)
        {
            if (userId.Equals(0))
                return BadRequest("O campo ID do Usuário é obrigatório.");

            _userAuthService.DeleteUser(userId);

            return Ok();
        }

        [HttpPost("signin")]
        [AllowAnonymous]
        public IActionResult SignIn([FromBody] UserVO user)
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

        [HttpPost("update-credentials/{userId}")]
        [Authorize(Roles = Role.Admin)]
        public IActionResult UpdateUserCredentials(long userId, [FromBody] UserUpdateVO user)
        {
            _logger.LogInformation("Attempting to update user: {userId}", userId);

            var updated = _userAuthService.UpdateUser(userId, user);

            _logger.LogInformation("User {userId} updated successfully", userId);

            return Ok(UserAccountVO.Parse(updated));
        }

        [HttpPut("{userId}/password")]
        [Authorize(Roles = Role.Admin)]
        public IActionResult SetUserPassword(long userId, [FromBody] UserPasswordVO data)
        {
            _logger.LogInformation("Attempting to set the password of user: {userId}", userId);

            var updated = _userAuthService.SetPassword(userId, User.Identity?.Name, data);

            _logger.LogInformation("Password of user {userId} set successfully", userId);

            return Ok(UserAccountVO.Parse(updated));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public IActionResult Refresh(
            [FromBody] TokenVO tokenDto)
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
        [Authorize(Roles = Role.Admin)]
        public IActionResult Create([FromBody] AccountCredentialsVO user)
        {
            if (user == null)
                return BadRequest("Invalid client request!");

            var result = _loginService.Create(user);

            return Ok(result);
        }

    }
}
