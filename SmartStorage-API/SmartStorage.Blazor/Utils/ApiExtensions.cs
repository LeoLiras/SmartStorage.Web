using SmartStorage.Blazor.Enums;
using SmartStorage_Shared.DTO;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Utils
{
    
    public class ApiExtensions
    {
        #region Properties

        private readonly HttpClient _http;

        #endregion

        #region Constructros

        public ApiExtensions(HttpClient http)
        {
            _http = http;
        }

        #endregion

        #region Endpoints

        //Produtos em estoque
        private string productsEndpoint = "api/storage/products/v1";

        private string shelvesEndpoint = "api/storage/shelf/v1";

        //Produtos alocados para venda (nas prateleiras)
        private string entersEndpoint = "api/storage/shelf/v1/allocation";

        private string salesEndpoint = "api/storage/sales/v1";

        private string employeesEndpoint = "api/storage/employees/v1";

        private string authEndpoint = "api/storage/auth";

        private string loginEndpoint = "api/storage/auth/signin";

        private string registerEndpoint = "api/storage/auth/create";

        private string updateCredentialsEndpoint = "api/storage/auth/update-credentials";

        #endregion

        #region Methods

        /// <summary>
        /// Requisição GET que retorna uma lista do modelos
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<List<T>?> Get<TVO, T>() where TVO : class
        {
            var url = ReturnEndpoint<TVO>();

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O campo URL é obrigatório.");

            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var parseListMethod = typeof(TVO).GetMethod("ParseList", new[] { typeof(List<TVO>) });

                if (parseListMethod == null)
                    throw new InvalidOperationException($"O método ParseList(List<{typeof(TVO).Name}>) não foi encontrado.");

                var modelList = (List<T>)parseListMethod.Invoke(null, new object[] { response })!;

                return modelList;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }  
        }

        /// <summary>
        /// Requisição GET que retorna uma lista de VO's
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<TVO>?> Get<TVO>() where TVO : class
        {
            var url = ReturnEndpoint<TVO>();

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O campo URL é obrigatório.");

            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<TVO>>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição GET que retorna um modelo com base na url e id fornecidos.
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<T?> GetById<TVO, T>(int id) where TVO : class
        {
            var url = ReturnEndpoint<TVO>();

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O campo URL é obrigatório.");

            if (id == 0)
                throw new ArgumentNullException(nameof(url), message: "O campo ID é obrigatório.");

            var response = await _http.GetAsync($"{url}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var parseMethod = typeof(TVO).GetMethod("Parse", new[] { typeof(TVO) });

                if (parseMethod == null)
                    throw new InvalidOperationException($"O método Parse({typeof(TVO).Name}) não foi encontrado.");

                var model = (T)parseMethod.Invoke(null, new object[] { response })!;

                return model;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição GET que retorna um modelo com base na url e id fornecidos.
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<TVO?> GetById<TVO>(int id) where TVO : class
        {
            var url = ReturnEndpoint<TVO>();

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O campo URL é obrigatório.");

            if (id == 0)
                throw new ArgumentNullException(nameof(url), message: "O campo ID é obrigatório.");

            var response = await _http.GetAsync($"{url}/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TVO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição GET que retorna um modelo com base na url e id fornecidos.
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<User> GetUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName), message: "O campo Nome de Usuário é obrigatório.");

            var url = $"{authEndpoint}?userName={Uri.EscapeDataString(userName)}";

            var response = await _http.GetAsync($"{url}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<User>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição GET para gerar o relatório excel de vendas do mês corrente
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ApiException"></exception>
        public async Task<byte[]> GetSalesReport(FileTypes fileType)
        {
            var url = "";

            if(fileType == FileTypes.Excel)
                url = $"{salesEndpoint}/export-excel";
            else
                url = $"{salesEndpoint}/export-pdf";

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição POST para criação de novos registros
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <param name="vo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TVO?> Post<TVO>(TVO vo) where TVO : class
        {
            var url = ReturnEndpoint<TVO>();

            if (vo == null)
                throw new ArgumentNullException(nameof(vo), message: "O parâmetro VO é obrigatório.");

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.PostAsJsonAsync(url, vo);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TVO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição POST para realizar o Login
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <param name="vo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TokenDTO?> PostSignin(UserDTO user)
        {
            var url = ReturnEndpoint<UserDTO>();

            if (user == null)
                throw new ArgumentNullException(nameof(user), message: "As credenciais do usuário são obrigatórias.");

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.PostAsJsonAsync(url, user);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TokenDTO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição POST para realizar cadastro de novo usuário
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <param name="vo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<AccountCredentialsDTO?> PostRegister(AccountCredentialsDTO credentials)
        {
            var url = ReturnEndpoint<AccountCredentialsDTO>();

            if (credentials == null)
                throw new ArgumentNullException(nameof(credentials), message: "As credenciais do usuário são obrigatórias.");

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.PostAsJsonAsync(url, credentials);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AccountCredentialsDTO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição POST para chamar a API do Gemini
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ApiException"></exception>
        public async Task<string?> PostAnalyseAI(string text)
        {
            var url = $"{salesEndpoint}/analyse/ai";

            if (text == null)
                throw new ArgumentNullException(nameof(text), message: "O parâmetro Texto é obrigatório.");

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.PostAsJsonAsync(url, text);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        /// <summary>
        /// Requisição PUT para atualização de registros existentes
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <param name="vo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TVO?> Put<TVO>(TVO vo, int id) where TVO : class
        {
            var url = ReturnEndpoint<TVO>();

            if (vo == null)
                throw new ArgumentNullException(nameof(vo), message: "O parâmetro VO é obrigatório.");

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.PutAsJsonAsync($"{url}/{id}", vo);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TVO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        public async Task<TVO?> Delete<TVO>(int id) where TVO : class
        {
            var url = ReturnEndpoint<TVO>();

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.DeleteAsync($"{url}/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TVO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        private string ReturnEndpoint<TVO>() where TVO : class
        {
            if (typeof(TVO) == typeof(EmployeeVO))
                return employeesEndpoint;

            else if (typeof(TVO) == typeof(EnterVO))
                return entersEndpoint;

            else if (typeof(TVO) == typeof(ProductVO))
                return productsEndpoint;

            else if (typeof(TVO) == typeof(SaleVO))
                return salesEndpoint;

            else if (typeof(TVO) == typeof(ShelfVO))
                return shelvesEndpoint;

            else if (typeof(TVO) == typeof(UserDTO))
                return loginEndpoint;

            else if (typeof(TVO) == typeof(AccountCredentialsDTO))
                return registerEndpoint;

            else if (typeof(TVO) == typeof(User))
                return updateCredentialsEndpoint;

            else
                return string.Empty;
        }

        #endregion

        #region Enums

        private enum ERequestTypes
        {
            GETAll = 0,
            GetId,
            POST,
            PUT,
            PATCH,
            DELETE
        }

        #endregion
    }

}
