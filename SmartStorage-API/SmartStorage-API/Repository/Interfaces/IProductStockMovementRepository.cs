using SmartStorage.Shared.Enum;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Repository.Interfaces
{
    public interface IProductStockMovementRepository
    {
        ProductStockMovement CreateNewStockMovement(
            int productId,
            int? shelfId,
            TipoMovimentacao type,
            int quantity,
            int? employeeId = null,
            string reason = null,
            decimal? shelfPrice = null,
            DateTime? date = null);

        List<ProductStockMovement> TransferProductBetweenLocations(
            int productId,
            int? fromShelfId,
            int? toShelfId,
            int quantity,
            TipoMovimentacao type = TipoMovimentacao.Alocacao,
            int? employeeId = null,
            string reason = null,
            decimal? shelfPrice = null,
            DateTime? date = null);

        int FindProductBalanceByLocation(int productId, int? shelfId);

        int FindProductTotalBalance(int productId);
    }
}
