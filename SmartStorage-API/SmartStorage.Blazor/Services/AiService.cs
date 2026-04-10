using SmartStorage.Blazor.Services.IServices;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _client;
        public const string BasePath = $"api/storage/ai/v1";

        public AiService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<string> CallAI(string message, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsJsonAsync($"{BasePath}", message);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
