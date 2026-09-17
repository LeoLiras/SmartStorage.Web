namespace SmartStorage.AIAPI.Repository.Interfaces
{
    public interface IAiRepository
    {
        Task<string> CallAISales(string text);
    }
}
