using SmartStorage.Shared.VO;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IAuthService
    {
        Task Login(UserVO user);
        Task Logout();
    }
}
