using SmartStorage_API.DTO;

namespace SmartStorage_API.Service
{
    public interface ISaleService
    {
        List<SaleDTO> FindAllSales();
        SaleDTO CreateNewSale(int productId, int saleQntd);
        SaleDTO UpdateSale(int saleId, int productId, int saleQntd);
    }
}
