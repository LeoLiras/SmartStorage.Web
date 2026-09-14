using SmartStorage_Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Utils.API
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
        public async Task<List<T>> Get<TVO, T>() where TVO : class
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
        public async Task<List<TVO>> Get<TVO>() where TVO : class
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
        /// Requisição GET paginada; o total vem no cabeçalho X-Total-Count.
        /// </summary>
        public async Task<(List<TVO> Items, int Total)> GetPage<TVO>(int page, int pageSize, string search = null) where TVO : class
        {
            var url = $"{ReturnEndpoint<TVO>()}?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search.Trim())}";

            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new ApiException((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var items = await response.Content.ReadFromJsonAsync<List<TVO>>() ?? new List<TVO>();

            var total = response.Headers.TryGetValues(Pagination.TotalCountHeader, out var values) && int.TryParse(values.FirstOrDefault(), out var parsed)
                ? parsed
                : items.Count;

            return (items, total);
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
        public async Task<T> GetById<TVO, T>(int id) where TVO : class
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
        public async Task<TVO> GetById<TVO>(int id) where TVO : class
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
        /// Requisição POST para criação de novos registros
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <param name="vo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TVO> Post<TVO>(TVO vo) where TVO : class
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

        public async Task<EnterVO> TransferProductToShelf(int enterId, ShelfTransferVO transfer)
        {
            if (transfer == null)
                throw new ArgumentNullException(nameof(transfer), message: "Os dados da transferência são obrigatórios.");

            var response = await _http.PostAsJsonAsync($"{entersEndpoint}/{enterId}/transfer", transfer);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EnterVO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new ApiException((int)response.StatusCode, error);
            }
        }

        public async Task<SaleVO> ReturnSale(int saleId, SaleReturnVO saleReturn)
        {
            if (saleReturn == null)
                throw new ArgumentNullException(nameof(saleReturn), message: "Os dados da devolução são obrigatórios.");

            var response = await _http.PostAsJsonAsync($"{salesEndpoint}/{saleId}/return", saleReturn);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SaleVO>();
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
        public async Task<TVO> Put<TVO>(TVO vo, int id) where TVO : class
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

        public async Task<TVO> Delete<TVO>(int id) where TVO : class
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
