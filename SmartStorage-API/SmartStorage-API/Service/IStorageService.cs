using SmartStorage_API.Model;
using SmartStorage_API.DTO;

namespace SmartStorage_API.Service
{
    public interface IStorageService
    {
        List<Product> FindAllProducts();
        List<SaleDTO> FindAllSales();
        List<ShelfDTO> FindAllShelves();
    }
}
