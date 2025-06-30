using SmartStorage_API.Data.VO;
using SmartStorage_API.DTO;

namespace SmartStorage_API.Service
{
    public interface IShelfService
    {
        List<EnterVO> FindAllProductsInShelves();
        EnterVO FindProductInShelfById(int enterId);
        List<ShelfVO> FindAllShelf();
        ShelfVO FindShelfById(int id);
        ShelfVO UpdateShelf(int shelfId, string shelfName);
        ShelfVO DeleteShelf(int shelfId);
        ShelfVO CreateNewShelf(ShelfVO newShelf);
        EnterVO AllocateProductToShelf(EnterVO newAllocation);
        EnterVO UndoAllocate(int enterId);

    }
}
