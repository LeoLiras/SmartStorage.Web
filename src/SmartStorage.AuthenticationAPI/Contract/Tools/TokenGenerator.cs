using Microsoft.IdentityModel.Tokens;
using SmartStorage.Shared.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartStorage.AuthenticationAPI.Contract.Tools
{
    public class TokenGenerator(TokenConfiguration configurations) : ITokenGenerator
    {
        private readonly TokenConfiguration _configurations = configurations;

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configurations.Secret));

            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                issuer: _configurations.Issuer,
                audience: _configurations.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    _configurations.Minutes),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = JwtRegisteredClaimNames.UniqueName,
                RoleClaimType = ClaimTypes.Role,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configurations.Secret)),
                ValidateLifetime = false
            };

            // Sem isso o handler renomeia unique_name para ClaimTypes.Name ao ler, o NameClaimType acima
            // não o acha e o Identity.Name volta nulo: o refresh respondia 401 para todo token.
            var tokenHandler = new JwtSecurityTokenHandler { MapInboundClaims = false };

            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out var securityToken);

            if (securityToken is not
                JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg
                    .Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))

                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
