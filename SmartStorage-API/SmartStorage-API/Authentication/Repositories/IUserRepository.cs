using SmartStorage_API.Authentication.Repositories.GenericRepository;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Repositories
{
    public interface IUserRepository

    {
        User? FindByUsername(string username);
    }
}
