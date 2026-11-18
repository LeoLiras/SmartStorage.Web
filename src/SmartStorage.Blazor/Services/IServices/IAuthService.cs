using SmartStorage.Shared.VO;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IAuthService
    {
        Task Login(UserVO user);
        Task Logout();
        Task<UserAccountVO> GetMyAccount();
        Task<List<UserAccountVO>> GetUsers();
        Task<UserAccountVO> GetUser(long userId);
        Task<AccountCredentialsVO> RegisterUser(AccountCredentialsVO account);
        Task<UserAccountVO> UpdateMyAccount(MyAccountUpdateVO data);
        Task<UserAccountVO> EditUser(long userId, UserUpdateVO data);
        Task<UserAccountVO> SetUserPassword(long userId, UserPasswordVO data);
        Task DeleteUser(int userId);
    }
}
