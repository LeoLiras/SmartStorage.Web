using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class AuthService : IAuthService
    {
        public const string BasePath = $"api/auth/v1";

        private readonly HttpClient http;
        private readonly AuthStateProvider authProvider;
        private readonly SessionExpiration session;

        public AuthService(HttpClient http, AuthStateProvider authProvider, SessionExpiration session)
        {
            this.http = http;
            this.authProvider = authProvider;
            this.session = session;
        }

        public async Task Login(UserVO user)
        {
            try
            {
                session.Reset();

                if (user == null)
                    throw new ArgumentNullException(nameof(user), message: "As credenciais do usuário são obrigatórias.");

                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                var responseSignIn = await http.PostAsJsonAsync($"{BasePath}/signin", user);

                if (responseSignIn.IsSuccessStatusCode)
                {
                    var token = await responseSignIn.Content.ReadFromJsonAsync<TokenVO>();

                    await authProvider.Login(token.AccessToken, token.RefreshToken);
                }
                else
                {
                    var error = await responseSignIn.Content.ReadAsStringAsync();

                    throw new ApiException((int)responseSignIn.StatusCode, error);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task Logout()
        {
            try
            {
                await authProvider.Logout();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UserAccountVO> GetMyAccount()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                // Sem parâmetro: a API identifica o usuário pelo token.
                var response = await http.GetFromJsonAsync<UserAccountVO>($"{BasePath}/my-account");

                if (response is not null)
                {
                    return response;
                }
                else
                {
                    throw new ApiException(404, "Usuário logado não encontrado.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<UserAccountVO>> GetUsers()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                return await http.GetFromJsonAsync<List<UserAccountVO>>($"{BasePath}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UserAccountVO> GetUser(long userId)
        {
            try
            {
                if (userId == 0)
                    throw new ArgumentNullException(nameof(userId), message: "O ID do usuário é obrigatório.");

                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                var response = await http.GetFromJsonAsync<UserAccountVO>($"{BasePath}/{userId}");

                if (response is not null)
                {
                    return response;
                }
                else
                {
                    throw new ApiException(404, $"Usuário não encontrado com o ID:{userId}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AccountCredentialsVO> RegisterUser(AccountCredentialsVO account)
        {
            try
            {
                if (account is null)
                    throw new ArgumentNullException(nameof(account), message: "Os dados do usuário são obrigatórios.");

                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                var response = await http.PostAsJsonAsync($"{BasePath}/create", account);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AccountCredentialsVO>();
                }
                else
                {
                    throw new ApiException(400, $"Erro ao criar usuário {account.Username}.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UserAccountVO> UpdateMyAccount(MyAccountUpdateVO data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data), message: "Os dados da conta são obrigatórios.");

            var response = await http.PutAsJsonAsync($"{BasePath}/my-account", data);

            return await response.ReadApiAsync<UserAccountVO>();
        }

        public async Task<UserAccountVO> EditUser(long userId, UserUpdateVO data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data), message: "Os dados do usuário são obrigatórios.");

            var response = await http.PostAsJsonAsync($"{BasePath}/update-credentials/{userId}", data);

            return await response.ReadApiAsync<UserAccountVO>();
        }

        public async Task<UserAccountVO> SetUserPassword(long userId, UserPasswordVO data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data), message: "A nova senha é obrigatória.");

            var response = await http.PutAsJsonAsync($"{BasePath}/{userId}/password", data);

            return await response.ReadApiAsync<UserAccountVO>();
        }

        public async Task DeleteUser(int userId)
        {
            try
            {
                if (userId == 0)
                    throw new ArgumentNullException(nameof(userId), message: "O ID do usuário é obrigatório.");

                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                var response = await http.DeleteAsync($"{BasePath}/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    throw new ApiException((int)response.StatusCode, error);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
