using System.Net.Http.Json;

namespace SmartStorage.Blazor.Utils
{
    public class ApiExtensions
    {
        private readonly HttpClient _http;

        public ApiExtensions(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Requisição GET que retorna uma lista do modelos
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<List<T>?> Get<TVO, T>(string url) where TVO : class
        {
            if(string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O campo URL é obrigatório.");

            var voList = await _http.GetFromJsonAsync<List<TVO>>(url);

            if (voList == null) 
                return null;

            var parseListMethod = typeof(TVO).GetMethod("ParseList", new[] { typeof(List<TVO>) });

            if (parseListMethod == null)
                throw new InvalidOperationException($"O método ParseList(List<{typeof(TVO).Name}>) não foi encontrado.");

            var modelList = (List<T>)parseListMethod.Invoke(null, new object[] { voList })!;

            return modelList;
        }

        /// <summary>
        /// Requisição GET que retorna uma lista de VO's
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<TVO>?> Get<TVO>(string url) where TVO : class
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O campo URL é obrigatório.");

            var voList = await _http.GetFromJsonAsync<List<TVO>>(url);

            if (voList == null)
                return null;

            return voList;
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
        public async Task<T?> GetWithId<TVO, T>(string url, int id) where TVO : class
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O campo URL é obrigatório.");

            if (id == 0)
                throw new ArgumentNullException(nameof(url), message: "O campo ID é obrigatório.");

            var vo = await _http.GetFromJsonAsync<TVO>($"{url}/{id}");

            if (vo == null)
                return default;

            var parseMethod = typeof(TVO).GetMethod("Parse", new[] { typeof(TVO) });

            if (parseMethod == null)
                throw new InvalidOperationException($"O método Parse({typeof(TVO).Name}) não foi encontrado.");

            var model = (T)parseMethod.Invoke(null, new object[] { vo })!;

            return model;
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
        public async Task<TVO?> GetWithId<TVO>(string url, int id) where TVO : class
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O campo URL é obrigatório.");

            if (id == 0)
                throw new ArgumentNullException(nameof(url), message: "O campo ID é obrigatório.");

            var vo = await _http.GetFromJsonAsync<TVO>($"{url}/{id}");

            if (vo == null)
                return default;

            return vo;
        }

        /// <summary>
        /// Requisição POST para criação de novos registros
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <param name="vo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TVO?> Post<TVO>(string url, TVO vo) where TVO : class
        {
            if (vo == null)
                throw new ArgumentNullException(nameof(vo), message: "O parâmetro VO é obrigatório.");

            if(string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.PostAsJsonAsync(url, vo);
            
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TVO>();

            return result;
        }

        /// <summary>
        /// Requisição PUT para atualização de registros existentes
        /// </summary>
        /// <typeparam name="TVO"></typeparam>
        /// <param name="url"></param>
        /// <param name="vo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TVO?> Put<TVO>(string url, TVO vo) where TVO : class
        {
            if (vo == null)
                throw new ArgumentNullException(nameof(vo), message: "O parâmetro VO é obrigatório.");

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), message: "O parâmetro URL é obrigatório.");

            var response = await _http.PutAsJsonAsync(url, vo);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TVO>();

            return result;
        }

    }

}
