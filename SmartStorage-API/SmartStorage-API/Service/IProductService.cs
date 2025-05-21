using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface IProductService
    {
        List<Product> FindAllProducts();
        Product FindProductById(int id);
        Product CreateNewProduct(ProductDTO product);
    }
}
