namespace SmartStorage.Blazor.Services.IServices
{
    public interface IAuthService
    {
        Task Login(string token);
        Task Logout();
    }
}
