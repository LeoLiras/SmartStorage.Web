using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage_Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class SaleService : ISaleService
    {
        #region Properties

        private readonly HttpClient _client;

        public const string BasePath = "api/storage/sales/v1";

        #endregion

        #region Constructors

        public SaleService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #endregion

        #region Methods

        public async Task<List<SaleVO>> GetSales()
        {
            var response = await _client.GetAsync(BasePath);

            return await response.ReadApiAsync<List<SaleVO>>();
        }

        public async Task<(List<SaleVO> Items, int Total)> GetSalesPage(int page, int pageSize, string search = null)
        {
            var response = await _client.GetAsync(BasePath.WithPage(page, pageSize, search));

            return await response.ReadApiPageAsync<SaleVO>();
        }

        public async Task<SaleVO> GetSaleById(int saleId)
        {
            var response = await _client.GetAsync($"{BasePath}/{saleId}");

            return await response.ReadApiAsync<SaleVO>();
        }

        public async Task<SaleVO> CreateSale(SaleVO sale)
        {
            ArgumentNullException.ThrowIfNull(sale);

            var response = await _client.PostAsJsonAsync(BasePath, sale);

            return await response.ReadApiAsync<SaleVO>();
        }

        public async Task<List<SaleVO>> CreateSales(SaleBatchVO batch)
        {
            ArgumentNullException.ThrowIfNull(batch);

            var response = await _client.PostAsJsonAsync($"{BasePath}/batch", batch);

            return await response.ReadApiAsync<List<SaleVO>>();
        }

        public async Task<SaleVO> CancelSale(int saleId)
        {
            var response = await _client.DeleteAsync($"{BasePath}/{saleId}");

            return await response.ReadApiAsync<SaleVO>();
        }

        public async Task<SaleVO> ReturnSale(int saleId, SaleReturnVO saleReturn)
        {
            ArgumentNullException.ThrowIfNull(saleReturn);

            var response = await _client.PostAsJsonAsync($"{BasePath}/{saleId}/return", saleReturn);

            return await response.ReadApiAsync<SaleVO>();
        }

        #endregion
    }
}
