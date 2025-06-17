using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface IShelfService
    {
        List<ShelfDTO> FindAllProductsInShelves();
        List<Shelf> FindAllShelf();
        Shelf FindShelfById(int id);
        Shelf UpdateShelf(int shelfId, string shelfName);
        Shelf DeleteShelf(int shelfId);
        Shelf CreateNewShelf(NewShelfDTO newShelf);
        Enter AllocateProductToShelf(AllocateProductToShelfDTO newAllocation);
        Enter UndoAllocate(int enterId);

    }
}
