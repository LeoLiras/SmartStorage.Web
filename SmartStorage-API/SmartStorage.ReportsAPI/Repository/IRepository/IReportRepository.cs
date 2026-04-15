namespace SmartStorage.ReportsAPI.Repository.IRepository
{
    public interface IReportRepository
    {
        byte[] GenerateExcel();
        Task<byte[]> GeneratePdf();
    }
}
