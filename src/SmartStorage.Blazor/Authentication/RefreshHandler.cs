using Microsoft.JSInterop;
using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Utils.Local_Storage;
using SmartStorage.Shared.VO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SmartStorage.Blazor.Authentication
{
    /// <summary>
    /// Renova o access token no 401 e repete a requisição. Fica por dentro do
    /// <see cref="SessionExpiredHandler"/>, para ele só ver o 401 que sobrar depois de o
    /// refresh ser recusado.
    /// </summary>
    public class RefreshHandler : DelegatingHandler
    {
        #region Properties

        // Sem AuthHandler, SessionExpiredHandler nem este handler: o 401 do próprio refresh
        // entraria de novo aqui e renovaria em laço.
        public const string ClientName = "Refresh";

        private readonly IHttpClientFactory httpClientFactory;
        private readonly IJSRuntime js;
        private readonly RefreshLock refreshLock;

        #endregion

        #region Constructors

        public RefreshHandler(IHttpClientFactory httpClientFactory, IJSRuntime js, RefreshLock refreshLock)
        {
            this.httpClientFactory = httpClientFactory;
            this.js = js;
            this.refreshLock = refreshLock;
        }

        #endregion

        #region Methods

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized
                || string.IsNullOrWhiteSpace(request.Headers.Authorization?.Parameter)
                || IsSignIn(request))
                return response;

            var accessToken = await RenewAsync(request.Headers.Authorization.Parameter, cancellationToken);

            if (accessToken is null)
                return response;

            response.Dispose();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await base.SendAsync(request, cancellationToken);
        }

        // O AuthService manda o token guardado também no signin: senha errada com uma sessão
        // vencida no navegador não pode virar refresh.
        private static bool IsSignIn(HttpRequestMessage request)
            => request.RequestUri.AbsolutePath.EndsWith("/signin", StringComparison.OrdinalIgnoreCase);

        private async Task<string> RenewAsync(string expiredAccessToken, CancellationToken cancellationToken)
        {
            await refreshLock.Semaphore.WaitAsync(cancellationToken);

            try
            {
                // Quem esperou na trava encontra o token já trocado por quem renovou antes dele.
                var currentAccessToken = await js.GetFromLocalStorage(AuthStateProvider.tokenKey);

                if (string.IsNullOrWhiteSpace(currentAccessToken) || expiredAccessToken == refreshLock.RefusedAccessToken)
                    return null;

                if (currentAccessToken != expiredAccessToken)
                    return currentAccessToken;

                var refreshToken = await js.GetFromLocalStorage(AuthStateProvider.refreshTokenKey);

                if (string.IsNullOrWhiteSpace(refreshToken))
                    return null;

                var http = httpClientFactory.CreateClient(ClientName);

                var response = await http.PostAsJsonAsync($"{AuthService.BasePath}/refresh",
                    new TokenVO { AccessToken = expiredAccessToken, RefreshToken = refreshToken },
                    cancellationToken);

                var token = response.IsSuccessStatusCode
                    ? await response.Content.ReadFromJsonAsync<TokenVO>(cancellationToken)
                    : null;

                if (string.IsNullOrWhiteSpace(token?.AccessToken))
                {
                    refreshLock.RefusedAccessToken = expiredAccessToken;
                    return null;
                }

                await js.SetInLocalStorage(AuthStateProvider.tokenKey, token.AccessToken);
                await js.SetInLocalStorage(AuthStateProvider.refreshTokenKey, token.RefreshToken);

                return token.AccessToken;
            }
            finally
            {
                refreshLock.Semaphore.Release();
            }
        }

        #endregion
    }
}
