using SmartStorage_Shared.DTO;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Services
{
    public interface ILoginService
    {
        TokenDTO? ValidateCredentials(UserDTO user);
        TokenDTO? ValidateCredentials(TokenDTO token);
        bool RevokeToken(string username);
        AccountCredentialsDTO Create(AccountCredentialsDTO user);
        User UpdateCredentials(User user);
    }
}
