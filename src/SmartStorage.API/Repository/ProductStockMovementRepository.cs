using SmartStorage.Shared.Enum;
using SmartStorage_API.Model.Context;
using SmartStorage_API.Repository.Interfaces;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Repository
{
    public class ProductStockMovementRepository : IProductStockMovementRepository
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Construtores

        public ProductStockMovementRepository(SmartStorageContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Métodos

        public ProductStockMovement CreateNewStockMovement(
            int productId,
            int? shelfId,
            TipoMovimentacao type,
            int quantity,
            string reason = null,
            decimal? shelfPrice = null,
            DateTime? date = null)
        {
            var userId = FindAuthenticatedUserId();

            var movement = ApplyMovementToBalance(productId, shelfId, type, quantity, userId, reason, shelfPrice, date);

            _context.SaveChanges();

            return movement;
        }

        public List<ProductStockMovement> TransferProductBetweenLocations(
            int productId,
            int? fromShelfId,
            int? toShelfId,
            int quantity,
            TipoMovimentacao type = TipoMovimentacao.Alocacao,
            string reason = null,
            decimal? shelfPrice = null,
            DateTime? date = null)
        {
            if (quantity <= 0)
                throw new Exception("A quantidade da transferência deve ser maior que zero.");

            if (fromShelfId == toShelfId)
                throw new Exception("A origem e o destino da transferência são o mesmo local.");

            var userId = FindAuthenticatedUserId();

            var movementDate = date ?? DateTime.Now;

            var movements = new List<ProductStockMovement>
            {
                ApplyMovementToBalance(productId, fromShelfId, type, -quantity, userId, reason, shelfPrice, movementDate),
                ApplyMovementToBalance(productId, toShelfId, type, quantity, userId, reason, shelfPrice, movementDate)
            };

            _context.SaveChanges();

            return movements;
        }

        public int FindProductBalanceByLocation(int productId, int? shelfId)
        {
            if (shelfId is null)
            {
                var product = FindProductById(productId);

                return product.ProQntd;
            }

            var enter = FindEnterByProductAndShelf(productId, shelfId.Value);

            return enter?.EntQntd ?? 0;
        }

        public int FindProductTotalBalance(int productId)
        {
            var product = FindProductById(productId);

            var inShelves = _context.Enters
                .Where(e => e.EntProId == productId)
                .Sum(e => (int?)e.EntQntd) ?? 0;

            return product.ProQntd + inShelves;
        }

        private ProductStockMovement ApplyMovementToBalance(
            int productId,
            int? shelfId,
            TipoMovimentacao type,
            int quantity,
            long userId,
            string reason,
            decimal? shelfPrice,
            DateTime? date)
        {
            if (quantity == 0)
                throw new Exception("A quantidade da movimentação não pode ser zero.");

            if (type == TipoMovimentacao.Ajuste && string.IsNullOrWhiteSpace(reason))
                throw new Exception("O motivo é obrigatório no ajuste de estoque.");

            var product = FindProductById(productId);

            var movementDate = date ?? DateTime.Now;

            if (shelfId is null)
            {
                if (product.ProQntd + quantity < 0)
                    throw new Exception($"Saldo insuficiente no depósito: há {product.ProQntd} e a movimentação pede {Math.Abs(quantity)}.");

                product.ProQntd += quantity;
            }
            else
            {
                var enter = FindEnterByProductAndShelf(productId, shelfId.Value);

                if (enter is null)
                {
                    if (quantity < 0)
                        throw new Exception($"Saldo insuficiente na prateleira {shelfId}: há 0 e a movimentação pede {Math.Abs(quantity)}.");

                    var shelf = _context.Shelves.FirstOrDefault(s => s.SheId == shelfId.Value)
                        ?? throw new Exception("Prateleira não encontrada na base de dados");

                    enter = new Enter
                    {
                        EntProId = productId,
                        EntSheId = shelf.SheId,
                        EntQntd = 0,
                        EntDateEnter = movementDate,
                        EntPrice = shelfPrice ?? 0
                    };

                    _context.Enters.Add(enter);
                }

                if (enter.EntQntd + quantity < 0)
                    throw new Exception($"Saldo insuficiente na prateleira {shelfId}: há {enter.EntQntd} e a movimentação pede {Math.Abs(quantity)}.");

                enter.EntQntd += quantity;

                if (shelfPrice.HasValue)
                    enter.EntPrice = shelfPrice.Value;
            }

            var movement = new ProductStockMovement
            {
                PsmProId = productId,
                PsmSheId = shelfId,
                PsmType = type,
                PsmQntd = quantity,
                PsmDate = movementDate,
                PsmUseId = userId,
                PsmReason = reason
            };

            _context.ProductStockMovements.Add(movement);

            return movement;
        }

        public long FindAuthenticatedUserId()
        {
            var username = _httpContextAccessor.HttpContext?.User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("Não foi possível identificar o usuário autenticado.");

            return _context.Users
                .Where(u => u.Username == username)
                .Select(u => (long?)u.Id)
                .FirstOrDefault()
                ?? throw new Exception("Usuário autenticado não encontrado na base de dados");
        }

        private Product FindProductById(int productId)
        {
            return _context.Products.FirstOrDefault(p => p.ProId == productId)
                ?? throw new Exception("Produto não encontrado na base de dados");
        }

        private Enter FindEnterByProductAndShelf(int productId, int shelfId)
        {
            return _context.Enters.FirstOrDefault(e => e.EntProId == productId && e.EntSheId == shelfId);
        }

        #endregion
    }
}
