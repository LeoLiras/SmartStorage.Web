using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Services
{
    public interface ILoginService
    {
        TokenVO? ValidateCredentials(UserVO user);
        TokenVO? ValidateCredentials(TokenVO token);
        bool RevokeToken(string username);
        AccountCredentialsVO Create(AccountCredentialsVO user);
        User UpdateCredentials(User user);
    }
}
