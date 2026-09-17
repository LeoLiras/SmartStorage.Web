using SmartStorage_Shared.VO;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Utils.API
{
    public static class HttpResponseExtensions
    {
        public static async Task EnsureApiSuccessAsync(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ApiException((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        public static async Task<T> ReadApiAsync<T>(this HttpResponseMessage response)
        {
            await response.EnsureApiSuccessAsync();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public static async Task<(List<T> Items, int Total)> ReadApiPageAsync<T>(this HttpResponseMessage response)
        {
            var items = await response.ReadApiAsync<List<T>>() ?? new List<T>();

            var total = response.Headers.TryGetValues(Pagination.TotalCountHeader, out var values) && int.TryParse(values.FirstOrDefault(), out var parsed)
                ? parsed
                : items.Count;

            return (items, total);
        }

        public static string WithPage(this string path, int page, int pageSize, string search)
        {
            var url = $"{path}?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search.Trim())}";

            return url;
        }
    }
}
