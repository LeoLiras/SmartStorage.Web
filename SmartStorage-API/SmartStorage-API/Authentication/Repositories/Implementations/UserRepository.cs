using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly SmartStorageContext _context;

        public UserRepository(SmartStorageContext context)
        {
            _context = context;
        }

        public User? FindByUsername(string username)
        {
            return _context.Users.SingleOrDefault(u => u.Username == username);
        }
    }
}
