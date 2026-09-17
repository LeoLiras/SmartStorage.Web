using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage_Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class InvoiceService : IInvoiceService
    {
        #region Properties

        private readonly HttpClient _client;

        public const string BasePath = "api/storage/invoices/v1";

        #endregion

        #region Constructors

        public InvoiceService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #endregion

        #region Methods

        public async Task<InvoicePreviewVO> PreviewInvoice(string xml)
        {
            var response = await _client.PostAsJsonAsync($"{BasePath}/preview", new InvoiceImportVO { Xml = xml });

            return await response.ReadApiAsync<InvoicePreviewVO>();
        }

        public async Task<InvoiceVO> ImportInvoice(InvoiceImportVO import)
        {
            ArgumentNullException.ThrowIfNull(import);

            var response = await _client.PostAsJsonAsync(BasePath, import);

            return await response.ReadApiAsync<InvoiceVO>();
        }

        #endregion
    }
}
