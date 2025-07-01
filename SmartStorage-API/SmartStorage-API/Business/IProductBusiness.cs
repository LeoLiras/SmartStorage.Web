using SmartStorage_API.Data.VO;

namespace SmartStorage_API.Service
{
    public interface IProductBusiness
    {
        List<ProductVO> FindAllProducts();
        ProductVO FindProductById(int id);
        ProductVO CreateNewProduct(ProductVO product);
        ProductVO UpdateProduct(int productId, ProductVO product);
        ProductVO DeleteProduct(int productId);
    }
}
