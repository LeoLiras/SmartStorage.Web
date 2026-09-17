using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface ISaleService
    {
        Task<List<SaleVO>> GetSales();
        Task<(List<SaleVO> Items, int Total)> GetSalesPage(int page, int pageSize, string search = null);
        Task<SaleVO> GetSaleById(int saleId);
        Task<SaleVO> CreateSale(SaleVO sale);
        Task<List<SaleVO>> CreateSales(SaleBatchVO batch);
        Task<SaleVO> CancelSale(int saleId);
        Task<SaleVO> ReturnSale(int saleId, SaleReturnVO saleReturn);
    }
}
