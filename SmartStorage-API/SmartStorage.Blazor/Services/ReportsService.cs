using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Shared.VO.AiService;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class ReportsService : IReportsService
    {
        #region Properties

        private readonly HttpClient _client;
        public const string BasePath = $"http://localhost:5005/api/storage/reports/v1";

        #endregion

        #region Constructors

        public ReportsService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #endregion

        #region Methods

        public async Task<byte[]> GenerateExcel()
        {
            var response = await _client.GetAsync($"{BasePath}/export-excel");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> GeneratePdf()
        {
            var response = await _client.GetAsync($"{BasePath}/export-pdf");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsByteArrayAsync();
        }

        #endregion
    }
}
