using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IProductService
    {
        Task<List<ProductVO>> GetProducts();
        Task<(List<ProductVO> Items, int Total)> GetProductsPage(int page, int pageSize, string search = null);
        Task<ProductVO> GetProductById(int productId);
        Task<ProductVO> CreateProduct(ProductVO product);
        Task<ProductVO> UpdateProduct(int productId, ProductVO product);
    }
}
