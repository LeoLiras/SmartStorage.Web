using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service
{
    public interface IInvoiceBusiness
    {
        InvoicePreviewVO PreviewInvoice(string xml);
        InvoiceVO ImportInvoice(InvoiceImportVO import);
    }
}
