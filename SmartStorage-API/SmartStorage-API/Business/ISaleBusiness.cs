using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service
{
    public interface ISaleBusiness
    {
        List<SaleVO> FindAllSales();
        SaleVO FindSaleById(int saleId);
        SaleVO CreateNewSale(int enterId, int saleQntd, DateTime dateSale);
        SaleVO UpdateSale(int saleId, int saleQntd);
        SaleVO DeleteSale(int saleId);
        byte[] GenerateExcel();
        Task<byte[]> GeneratePdf();
    }
}
