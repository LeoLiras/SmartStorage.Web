using SmartStorage_API.Authentication.Contract;
using SmartStorage_API.Authentication.Repositories;
using SmartStorage_Shared.DTO;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Services.Implementations
{
    public class UserAuthServiceImplementation(IUserRepository repository, IPasswordHasher passwordHasher) : IUserAuthService

    {
        private readonly IUserRepository _repository = repository;

        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        public User? FindByUsername(string username)
        {
            return _repository.FindByUsername(username);
        }
        public User Create(AccountCredentialsDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var entity = new User
            {
                Username = dto.Username,
                FullName = dto.Fullname,
                Password = _passwordHasher.Hash(dto.Password),
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null
            };

            return _repository.Create(entity);
        }

        public bool RevokeToken(string username)
        {
            var user = _repository.FindByUsername(username);
            if (user == null) return false;
            user.RefreshToken = null;
            _repository.Update(user);
            return true;
        }

        public User Update(User user)
        {
            return _repository.Update(user);
        }

    }
}
