using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Services
{
    public class AuthService : IAuthService
    {
        public const string BasePath = $"api/auth/v1";

        private readonly HttpClient http;
        private readonly AuthStateProvider authProvider;

        public AuthService(HttpClient http, AuthStateProvider authProvider)
        {
            this.http = http;
            this.authProvider = authProvider;
        }

        public async Task Login(UserVO user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user), message: "As credenciais do usuário são obrigatórias.");

                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                var responseSignIn = await http.PostAsJsonAsync($"{BasePath}/signin", user);

                if (responseSignIn.IsSuccessStatusCode)
                {
                    var token = await responseSignIn.Content.ReadFromJsonAsync<TokenVO>();

                    await authProvider.Login(token.AccessToken);
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

        public async Task<User> GetUser(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    throw new ArgumentNullException(nameof(userName), message: "As credenciais do usuário são obrigatórias.");

                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                var response = await http.GetFromJsonAsync<User>($"{BasePath}/user-by-username?userName={Uri.EscapeDataString(userName)}");

                if (response is not null)
                {
                    return response;
                }
                else
                {
                    throw new ApiException(404, $"Usuário não encontrado com o user-name:{userName}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<User>> GetUser()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BasePath))
                    throw new ArgumentNullException(nameof(BasePath), message: "O parâmetro URL é obrigatório.");

                return await http.GetFromJsonAsync<List<User>>($"{BasePath}");  
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
