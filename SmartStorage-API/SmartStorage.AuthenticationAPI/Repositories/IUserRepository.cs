using SmartStorage.AuthenticationAPI.Repositories.GenericRepository;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Repositories
{
    public interface IUserRepository : IRepository<User>

    {
        User FindByUsername(string username);
    }
}
