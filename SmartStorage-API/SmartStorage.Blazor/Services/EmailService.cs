using SmartStorage.Blazor.Services.IServices;
using SmartStorage_Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _client;
        public const string BasePath = $"http://localhost:5007/api/storage/email/v1";

        public EmailService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<bool> NewProductEmail(ProductVO product)
        {
            var response = await _client.PostAsJsonAsync($"{BasePath}/new-product", product);

            return response.IsSuccessStatusCode;
        }
    }
}
