using SmartStorage_API.Model.Context;
using SmartStorage_API.RabbitMQSender;
using SmartStorage_API.Repository.Interfaces;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service.Implementations
{
    public class StockAlertBusinessImplementation : IStockAlertBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly IProductStockMovementRepository _movementRepository;

        private readonly IRabbitMQMessageSender _messageSender;

        private readonly ILogger<StockAlertBusinessImplementation> _logger;

        #endregion

        #region Construtores

        public StockAlertBusinessImplementation(
            SmartStorageContext context,
            IProductStockMovementRepository movementRepository,
            IRabbitMQMessageSender messageSender,
            ILogger<StockAlertBusinessImplementation> logger)
        {
            _context = context;
            _movementRepository = movementRepository;
            _messageSender = messageSender;
            _logger = logger;
        }

        #endregion

        #region Métodos

        public void NotifyIfBelowMinimum(int productId, int totalBefore, string origin)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProId == productId);

            if (product is null || product.ProMinimumStock <= 0)
                return;

            var totalAfter = _movementRepository.FindProductTotalBalance(productId);

            if (totalBefore < product.ProMinimumStock || totalAfter >= product.ProMinimumStock)
                return;

            var alert = new LowStockAlertVO
            {
                ProductId = product.ProId,
                ProductName = product.ProName,
                MinimumStock = product.ProMinimumStock,
                TotalQntd = totalAfter,
                WarehouseQntd = product.ProQntd,
                ShelvesQntd = totalAfter - product.ProQntd,
                Origin = origin,
            };

            try
            {
                _messageSender.SendMessage(alert, LowStockAlertVO.QueueName);

                _logger.LogInformation("Alerta de estoque mínimo publicado para o produto {ProductId}: total {Total}, mínimo {Minimum}, origem {Origin}.",
                    product.ProId, totalAfter, product.ProMinimumStock, origin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar o alerta de estoque mínimo do produto {ProductId}.", product.ProId);
            }
        }

        #endregion
    }
}
