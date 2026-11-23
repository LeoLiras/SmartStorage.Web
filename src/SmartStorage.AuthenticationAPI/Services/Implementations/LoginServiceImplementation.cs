using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.Shared.Auth;
using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;
using System.Security.Claims;

namespace SmartStorage.AuthenticationAPI.Services.Implementations
{
    public class LoginServiceImplementation : ILoginService
    {
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

        private readonly IUserAuthService _userAuthService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenService;
        private readonly TokenConfiguration _configurations;

        public LoginServiceImplementation(
            IUserAuthService userAuthService,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenService,
            TokenConfiguration configurations)
        {
            _userAuthService = userAuthService;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _configurations = configurations;
        }

        public TokenVO ValidateCredentials(UserVO userDto)
        {
            var user = _userAuthService.FindByUsername(userDto.Username);

            if (user == null) return null;

            if (!_passwordHasher.Verify(userDto.Password, user.Password))
                return null;

            return GenerateToken(user, DateTime.Now.AddHours(_configurations.SessionHours));
        }

        public TokenVO ValidateCredentials(TokenVO token)
        {
            ClaimsPrincipal principal;

            // Assinatura errada e texto que nem é JWT lançam do validador; são o cliente
            // apresentando credencial ruim, então respondem 401 como o refresh token errado.
            try
            {
                principal = _tokenService.GetPrincipalFromExpiredToken(token.AccessToken);
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                return null;
            }

            var username = principal.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username)) return null;

            var user = _userAuthService.FindByUsername(username);

            // O revoke zera o RefreshToken: sem a checagem de vazio, um refresh token nulo no corpo
            // conferia com o do banco e renovava a sessão de quem já tinha saído.
            if (user == null ||
                string.IsNullOrEmpty(token.RefreshToken) ||
                user.RefreshToken != token.RefreshToken ||
                user.RefreshTokenExpiryTime is not DateTime sessionEnd ||
                sessionEnd <= DateTime.Now
                )
                return null;

            // As claims saem do usuário no banco, e não do token antigo, para a troca de papel valer já neste refresh.
            // O fim da sessão é o do login: o refresh troca o refresh token sem estender o prazo (D-23).
            return GenerateToken(user, sessionEnd);
        }

        public AccountCredentialsVO Create(AccountCredentialsVO dto)
        {
            var user = _userAuthService.Create(dto);

            return new AccountCredentialsVO
            {
                Username = user.Username,
                Fullname = user.FullName,
                Password = "************"
            };
        }

        public bool RevokeToken(string username)
        {
            return _userAuthService.RevokeToken(username);
        }

        private TokenVO GenerateToken(User user, DateTime sessionEnd)
        {
            var createdDate = DateTime.Now;

            // O que vier primeiro: com 60 minutos fixos, um refresh perto do fim levaria a sessão a até 10 horas.
            var expirationDate = createdDate.AddMinutes(_configurations.Minutes);
            if (sessionEnd < expirationDate)
                expirationDate = sessionEnd;

            List<Claim> claims =
                [
                    new Claim(JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString("N")),

                    new Claim(JwtRegisteredClaimNames.UniqueName,
                        user.Username),

                    new Claim(ClaimTypes.Role,
                        user.UseType == 0 ? Role.Client : Role.Admin)
                ];

            var accessToken = _tokenService.GenerateAccessToken(claims, expirationDate);

            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = sessionEnd;

            _userAuthService.Update(user);

            return new TokenVO
            {
                Authenticated = true,
                Created = createdDate.ToString(DATE_FORMAT),
                Expiration = expirationDate.ToString(DATE_FORMAT),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

    }
}
