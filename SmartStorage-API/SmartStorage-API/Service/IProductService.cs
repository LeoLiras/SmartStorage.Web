using SmartStorage_API.Data.VO;
using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface IProductService
    {
        List<ProductVO> FindAllProducts();
        ProductVO FindProductById(int id);
        ProductVO CreateNewProduct(ProductVO product);
        ProductVO UpdateProduct(int productId, ProductVO product);
        ProductVO DeleteProduct(int productId);
    }
}
