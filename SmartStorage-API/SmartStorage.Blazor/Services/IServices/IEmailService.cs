using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IEmailService
    {
        Task<bool> NewProductEmail(ProductVO product);
    }
}
