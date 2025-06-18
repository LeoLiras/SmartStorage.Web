using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface ISaleService
    {
        List<SaleDTO> FindAllSales();
        SaleDTO FindSaleById(int saleId);
        SaleDTO CreateNewSale(int productId, int saleQntd);
        SaleDTO UpdateSale(int saleId, int saleQntd);
        Sale DeleteSale(int saleId);
    }
}
