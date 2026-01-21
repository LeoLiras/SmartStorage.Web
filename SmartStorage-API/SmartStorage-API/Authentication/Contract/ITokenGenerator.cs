using System.Security.Claims;

namespace SmartStorage_API.Authentication.Contract
{
    public interface ITokenGenerator
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
