using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IInvoiceService
    {
        Task<InvoicePreviewVO> PreviewInvoice(string xml);
        Task<InvoiceVO> ImportInvoice(InvoiceImportVO import);
    }
}
