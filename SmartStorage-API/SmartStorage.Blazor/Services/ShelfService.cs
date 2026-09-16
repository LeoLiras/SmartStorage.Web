using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage_Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class ShelfService : IShelfService
    {
        #region Properties

        private readonly HttpClient _client;

        public const string BasePath = "api/storage/shelf/v1";

        public const string AllocationPath = $"{BasePath}/allocation";

        #endregion

        #region Constructors

        public ShelfService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #endregion

        #region Methods

        public async Task<List<ShelfVO>> GetShelves()
        {
            var response = await _client.GetAsync(BasePath);

            return await response.ReadApiAsync<List<ShelfVO>>();
        }

        public async Task<List<EnterVO>> GetAllocations()
        {
            var response = await _client.GetAsync(AllocationPath);

            return await response.ReadApiAsync<List<EnterVO>>();
        }

        public async Task<(List<EnterVO> Items, int Total)> GetAllocationsPage(int page, int pageSize, string search = null)
        {
            var response = await _client.GetAsync(AllocationPath.WithPage(page, pageSize, search));

            return await response.ReadApiPageAsync<EnterVO>();
        }

        public async Task<EnterVO> GetAllocationById(int enterId)
        {
            var response = await _client.GetAsync($"{AllocationPath}/{enterId}");

            return await response.ReadApiAsync<EnterVO>();
        }

        public async Task<EnterVO> AllocateProduct(EnterVO allocation)
        {
            ArgumentNullException.ThrowIfNull(allocation);

            var response = await _client.PostAsJsonAsync(AllocationPath, allocation);

            return await response.ReadApiAsync<EnterVO>();
        }

        public async Task<List<EnterVO>> AllocateProducts(AllocationBatchVO batch)
        {
            ArgumentNullException.ThrowIfNull(batch);

            var response = await _client.PostAsJsonAsync($"{AllocationPath}/batch", batch);

            return await response.ReadApiAsync<List<EnterVO>>();
        }

        public async Task<EnterVO> UndoAllocation(int enterId)
        {
            var response = await _client.PutAsync($"{AllocationPath}/{enterId}", null);

            return await response.ReadApiAsync<EnterVO>();
        }

        public async Task<EnterVO> TransferAllocation(int enterId, ShelfTransferVO transfer)
        {
            ArgumentNullException.ThrowIfNull(transfer);

            var response = await _client.PostAsJsonAsync($"{AllocationPath}/{enterId}/transfer", transfer);

            return await response.ReadApiAsync<EnterVO>();
        }

        #endregion
    }
}
