using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface IShelfService
    {
        List<ShelfDTO> FindAllProductsInShelves();
        List<Shelf> FindAllShelf();
        Enter AllocateProductToShelf(AllocateProductToShelfDTO newAllocation);
    }
}
