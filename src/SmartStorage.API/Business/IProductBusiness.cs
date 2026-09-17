using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service
{
    public interface IProductBusiness
    {
        List<ProductVO> FindAllProducts();
        (List<ProductVO> Items, int Total) FindProductsPage(int page, int pageSize, string search);
        ProductVO FindProductById(int id);
        ProductVO CreateNewProduct(ProductVO product);
        ProductVO UpdateProduct(int productId, ProductVO product);
        ProductVO AdjustProductStock(int productId, int newQuantity, string reason);
    }
}
