using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Services
{
    public interface IUserAuthService
    {
        User FindByUsername(string username);
        User FindUserById(int userId);
        List<User> FindAllUsers();
        User Create(AccountCredentialsVO dto);
        bool RevokeToken(string username);
        User Update(User user);
        User UpdateUser(long userId, UserUpdateVO data);
        User UpdateMyAccount(string username, MyAccountUpdateVO data);

        void DeleteUser(int userId);
    }
}
