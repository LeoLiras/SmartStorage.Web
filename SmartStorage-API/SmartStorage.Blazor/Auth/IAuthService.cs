namespace SmartStorage.Blazor.Auth
{
    public interface IAuthService
    {
        Task Login(string token);
        Task Logout();
    }
}
