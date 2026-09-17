using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace SmartStorage.Blazor.Authentication
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly IJSRuntime js;

        public AuthHandler(IJSRuntime jsRuntime)
        {
            js = jsRuntime;
        }

        protected override async Task<HttpResponseMessage>SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await js.InvokeAsync<string>(
                "localStorage.getItem",
                "tokenKey");

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
