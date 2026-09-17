using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IShelfService
    {
        Task<List<ShelfVO>> GetShelves();
        Task<List<EnterVO>> GetAllocations();
        Task<(List<EnterVO> Items, int Total)> GetAllocationsPage(int page, int pageSize, string search = null);
        Task<EnterVO> GetAllocationById(int enterId);
        Task<EnterVO> AllocateProduct(EnterVO allocation);
        Task<List<EnterVO>> AllocateProducts(AllocationBatchVO batch);
        Task<EnterVO> UndoAllocation(int enterId);
        Task<EnterVO> TransferAllocation(int enterId, ShelfTransferVO transfer);
        Task<List<EnterVO>> CountInventory(int shelfId, InventoryCountVO count);
    }
}
