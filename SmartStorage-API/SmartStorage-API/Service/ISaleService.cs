using SmartStorage_API.DTO;

namespace SmartStorage_API.Service
{
    public interface ISaleService
    {
        List<SaleDTO> FindAllSales();
        SaleDTO CreateNewSale(SaleDTO newSale);
    }
}
