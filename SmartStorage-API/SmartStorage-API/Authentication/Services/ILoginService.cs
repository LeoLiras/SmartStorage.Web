using SmartStorage_Shared.DTO;

namespace SmartStorage_API.Authentication.Services
{
    public interface ILoginService
    {
        TokenDTO? ValidateCredentials(UserDTO user);
        TokenDTO? ValidateCredentials(TokenDTO token);
        bool RevokeToken(string username);
        AccountCredentialsDTO Create(AccountCredentialsDTO user);
    }
}
