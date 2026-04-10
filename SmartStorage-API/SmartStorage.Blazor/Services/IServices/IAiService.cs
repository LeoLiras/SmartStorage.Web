namespace SmartStorage.Blazor.Services.IServices
{
    public interface IAiService
    {
        Task<string> CallAI(string message, string token);
    }
}
