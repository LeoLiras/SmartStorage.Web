using SmartStorage_API.Authentication.DTO_s;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Services
{
    public interface IUserAuthService
    {
        User? FindByUsername(string username);
        User Create(AccountCredentialsDTO dto);
        bool RevokeToken(string username);
        User Update(User user);
    }
}
