using SmartStorage.AuthenticationAPI.Repositories.GenericRepository;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Repositories.Implementations
{
    public class UserRepository(SmartStorageContext context) : GenericRepository<User>(context), IUserRepository
    {
        public User FindByUsername(string username)
        {
            return _context.Users.SingleOrDefault(u => u.Username == username);
        }

        public bool HasStockMovements(long userId)
        {
            return _context.ProductStockMovements.Any(m => m.PsmUseId == userId);
        }
    }
}
