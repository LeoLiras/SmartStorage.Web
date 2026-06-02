using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IAuthService
    {
        Task Login(UserVO user);
        Task Logout();
        Task<User> GetUser(string userName);
        Task<List<User>> GetUser();
        Task<User> GetUser(int userId);
    }
}
