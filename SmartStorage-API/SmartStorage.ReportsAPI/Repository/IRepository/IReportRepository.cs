namespace SmartStorage.ReportsAPI.Repository.IRepository
{
    public interface IReportRepository
    {
        Task<byte[]> GenerateExcel();
        Task<byte[]> GeneratePdf();
    }
}
