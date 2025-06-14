using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface IShelfService
    {
        List<ShelfDTO> FindAllProductsInShelves();
        Shelf FindShelfById(int id);
        List<Shelf> FindAllShelf();
        Shelf UpdateShelf(int shelfId, string shelfName);
        Shelf DeleteShelf(int shelfId);
        Shelf CreateNewShelf(NewShelfDTO newShelf);
        Enter AllocateProductToShelf(AllocateProductToShelfDTO newAllocation);
        Enter UndoAllocate(int enterId);

    }
}
