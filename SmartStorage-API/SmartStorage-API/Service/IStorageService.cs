using SmartStorage_API.Model;
using SmartStorage_API.DTO;

namespace SmartStorage_API.Service
{
    public interface IStorageService
    {
        Product CreateNewProduct(ProductDTO product);
        List<Product> FindAllProducts();
        Product FindProductById(int id);
        List<SaleDTO> FindAllSales();
        SaleDTO CreateNewSale(SaleDTO newSale);
        List<ShelfDTO> FindAllProductsInShelves();
        List<Shelf> FindAllShelf();
        Enter AllocateProductToShelf(AllocateProductToShelfDTO newAllocation);
        List<Employee> FindAllEmployees();
        Employee RegisterNewEmployee(EmployeeDTO employee);
    }
}
