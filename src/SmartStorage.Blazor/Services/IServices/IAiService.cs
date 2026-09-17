using SmartStorage.Shared.VO.AiService;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IAiService
    {
        Task<string> CallAI(string text);
    }
}
