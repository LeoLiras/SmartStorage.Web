using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Shared.VO.AiService;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _client;
        public const string BasePath = $"http://localhost:5003/api/storage/ai/v1";

        public AiService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<string> CallAI(string text)
        {
            var request = new AiRequest
            {
                aiQuestion = text
            };

            var response = await _client.PostAsJsonAsync($"{BasePath}/analyse-sales", request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
