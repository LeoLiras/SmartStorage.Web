namespace SmartStorage.Blazor.Services.IServices
{
    public interface IReportsService
    {
        Task<byte[]> GenerateExcel();
        Task<byte[]> GeneratePdf();
    }
}
