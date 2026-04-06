using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Services
{
    public interface IUserAuthService
    {
        User? FindByUsername(string username);
        User? FindUserById(int userId);
        List<User> FindAllUsers();
        User Create(AccountCredentialsVO dto);
        bool RevokeToken(string username);
        User Update(User user);
        User UpdateCredentials(User user);

        void DeleteUser(int userId);
    }
}
