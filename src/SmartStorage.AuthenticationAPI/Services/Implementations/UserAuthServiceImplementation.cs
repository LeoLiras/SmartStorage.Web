using Microsoft.EntityFrameworkCore;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.AuthenticationAPI.Repositories;
using SmartStorage.Shared.VO;
using SmartStorage.Shared.Validation;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Services.Implementations
{
    public class UserAuthServiceImplementation(IUserRepository repository, IPasswordHasher passwordHasher) : IUserAuthService

    {
        private readonly IUserRepository _repository = repository;

        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        public User FindByUsername(string username)
        {
            return _repository.FindByUsername(username);
        }

        public User FindUserById(int userId)
        {
            var user = _repository.FindById(userId);

            if (user is null)
                throw new BusinessException("Usuário não encontrado com o ID informado");

            return user;
        }

        public List<User> FindAllUsers()
        {
            return _repository.FindAll();
        }

        private const string DuplicateUsernameMessage = "Já existe um usuário com esse nome.";

        private const string SamePasswordMessage = "A nova senha precisa ser diferente da atual.";

        private static void EnsurePasswordPolicy(string password)
        {
            var reason = PasswordPolicy.Validate(password);

            if (reason is not null)
                throw new BusinessException(reason);
        }

        private void EnsureNewPassword(User user, string newPassword)
        {
            EnsurePasswordPolicy(newPassword);

            // Pelo hash, porque a senha atual só existe como hash no banco.
            if (_passwordHasher.Verify(newPassword, user.Password))
                throw new BusinessException(SamePasswordMessage);
        }

        public User Create(AccountCredentialsVO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            EnsurePasswordPolicy(dto.Password);

            if (_repository.FindByUsername(dto.Username) is not null)
                throw new BusinessException(DuplicateUsernameMessage);

            var entity = new User
            {
                Username = dto.Username,
                FullName = dto.Fullname,
                UseType = dto.Type,
                Password = _passwordHasher.Hash(dto.Password),
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null
            };

            try
            {
                return _repository.Create(entity);
            }
            // Dois cadastros do mesmo nome ao mesmo tempo passam os dois pela consulta acima;
            // quem barra o segundo é o índice único, e só essa violação vira recusa de regra.
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UQ_users_user_name") == true)
            {
                throw new BusinessException(DuplicateUsernameMessage);
            }
        }

        public User UpdateUser(long userId, UserUpdateVO data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var user = _repository.FindById(userId);

            if (user is null)
                throw new BusinessException("Usuário não encontrado com o ID informado");

            user.FullName = data.FullName;
            user.UseType = data.Type;

            return _repository.Update(user);
        }

        public User SetPassword(long userId, string requesterUsername, UserPasswordVO data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (string.IsNullOrWhiteSpace(data.NewPassword))
                throw new BusinessException("A nova senha é obrigatória.");

            var user = _repository.FindById(userId);

            if (user is null)
                throw new BusinessException("Usuário não encontrado com o ID informado");

            // A própria senha só muda com a atual, pelo my-account (RF-03a).
            if (user.Username == requesterUsername)
                throw new BusinessException("Para alterar a própria senha, use a sua conta.");

            EnsureNewPassword(user, data.NewPassword);

            user.Password = _passwordHasher.Hash(data.NewPassword);

            return _repository.Update(user);
        }

        public User UpdateMyAccount(string username, MyAccountUpdateVO data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var user = _repository.FindByUsername(username);

            if (user is null)
                throw new BusinessException("Usuário não existe.");

            if (!_passwordHasher.Verify(data.CurrentPassword, user.Password))
                throw new BusinessException("A senha atual está incorreta.");

            if (!string.IsNullOrWhiteSpace(data.NewPassword))
            {
                EnsureNewPassword(user, data.NewPassword);

                user.Password = _passwordHasher.Hash(data.NewPassword);
            }

            user.FullName = data.FullName;

            return _repository.Update(user);
        }

        public User Update(User user)
        {
            return _repository.Update(user);
        }

        public bool RevokeToken(string username)
        {
            var user = _repository.FindByUsername(username);
            if (user == null) return false;
            user.RefreshToken = null;
            _repository.Update(user);
            return true;
        }

        public void DeleteUser(int userId)
        {
            if (_repository.HasStockMovements(userId))
                throw new BusinessException("Não é possível excluir o usuário pois há movimentações de estoque registradas por ele.");

            _repository.Delete(userId);
        }
    }
}
