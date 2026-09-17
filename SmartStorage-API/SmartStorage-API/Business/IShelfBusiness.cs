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
        ShelfVO UpdateShelf(int shelfId, ShelfVO shelf);
        ShelfVO DeleteShelf(int shelfId);
        ShelfVO CreateNewShelf(ShelfVO newShelf);
        EnterVO AllocateProductToShelf(EnterVO newAllocation);
        List<EnterVO> AllocateProductsToShelves(List<AllocationBatchItemVO> items);
        EnterVO UndoAllocate(int enterId);
        EnterVO TransferProductToShelf(int enterId, int toShelfId);
        List<EnterVO> CountShelfInventory(int shelfId, InventoryCountVO count);

    }
}
