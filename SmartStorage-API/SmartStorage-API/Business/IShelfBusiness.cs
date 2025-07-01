using SmartStorage_API.Data.VO;

namespace SmartStorage_API.Service
{
    public interface IShelfBusiness
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
