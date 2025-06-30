using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface ISaleService
    {
        List<SaleVO> FindAllSales();
        SaleVO FindSaleById(int saleId);
        SaleVO CreateNewSale(int productId, int saleQntd);
        SaleVO UpdateSale(int saleId, int saleQntd);
        SaleVO DeleteSale(int saleId);
    }
}
