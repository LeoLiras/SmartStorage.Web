using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage_Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class ProductService : IProductService
    {
        #region Properties

        private readonly HttpClient _client;

        public const string BasePath = "api/storage/products/v1";

        #endregion

        #region Constructors

        public ProductService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #endregion

        #region Methods

        public async Task<List<ProductVO>> GetProducts()
        {
            var response = await _client.GetAsync(BasePath);

            return await response.ReadApiAsync<List<ProductVO>>();
        }

        public async Task<(List<ProductVO> Items, int Total)> GetProductsPage(int page, int pageSize, string search = null)
        {
            var response = await _client.GetAsync(BasePath.WithPage(page, pageSize, search));

            return await response.ReadApiPageAsync<ProductVO>();
        }

        public async Task<ProductVO> GetProductById(int productId)
        {
            var response = await _client.GetAsync($"{BasePath}/{productId}");

            return await response.ReadApiAsync<ProductVO>();
        }

        public async Task<ProductVO> CreateProduct(ProductVO product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var response = await _client.PostAsJsonAsync(BasePath, product);

            return await response.ReadApiAsync<ProductVO>();
        }

        public async Task<ProductVO> UpdateProduct(int productId, ProductVO product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var response = await _client.PutAsJsonAsync($"{BasePath}/{productId}", product);

            return await response.ReadApiAsync<ProductVO>();
        }

        #endregion
    }
}
