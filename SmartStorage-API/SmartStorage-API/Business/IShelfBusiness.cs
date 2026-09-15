using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service
{
    public interface IShelfBusiness
    {
        List<EnterVO> FindAllProductsInShelves();
        (List<EnterVO> Items, int Total) FindProductsInShelvesPage(int page, int pageSize, string search);
        EnterVO FindProductInShelfById(int enterId);
        List<ShelfVO> FindAllShelf();
        ShelfVO FindShelfById(int id);
        ShelfVO UpdateShelf(int shelfId, string shelfName);
        ShelfVO DeleteShelf(int shelfId);
        ShelfVO CreateNewShelf(ShelfVO newShelf);
        EnterVO AllocateProductToShelf(EnterVO newAllocation);
        EnterVO UndoAllocate(int enterId);
        EnterVO TransferProductToShelf(int enterId, int toShelfId);

    }
}
