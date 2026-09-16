using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service
{
    public interface ISaleBusiness
    {
        List<SaleVO> FindAllSales();
        (List<SaleVO> Items, int Total) FindSalesPage(int page, int pageSize, string search);
        SaleVO FindSaleById(int saleId);
        SaleVO CreateNewSale(int enterId, int saleQntd, DateTime dateSale);
        List<SaleVO> CreateNewSales(List<SaleBatchItemVO> items, DateTime dateSale);
        SaleVO UpdateSale(int saleId, int saleQntd);
        SaleVO DeleteSale(int saleId);
        SaleVO ReturnSale(int saleId, int quantity);
    }
}
