using SmartStorage_Shared.DTO;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Services
{
    public interface IUserAuthService
    {
        User? FindByUsername(string username);
        User Create(AccountCredentialsDTO dto);
        bool RevokeToken(string username);
        User Update(User user);
        User UpdateCredentials(User user);
    }
}
