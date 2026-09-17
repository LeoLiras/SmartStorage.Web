using SmartStorage.MessageBus;

namespace SmartStorage_Shared.VO
{
    public class LowStockAlertVO : BaseMessage
    {
        public const string QueueName = "lowstockemailqueue";

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int MinimumStock { get; set; }

        public int TotalQntd { get; set; }

        public int WarehouseQntd { get; set; }

        public int ShelvesQntd { get; set; }

        public string Origin { get; set; }
    }
}
