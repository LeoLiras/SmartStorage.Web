using SmartStorage_Shared.VO;

namespace SmartStorage.EmailAPI.Repository.Interfaces
{
    public interface IEmailRepository
    {
        Task NewProductEmail(ProductVO product);
    }
}
