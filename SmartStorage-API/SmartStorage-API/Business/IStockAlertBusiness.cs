namespace SmartStorage_API.Service
{
    public interface IStockAlertBusiness
    {
        void NotifyIfBelowMinimum(int productId, int totalBefore, string origin);
    }
}
