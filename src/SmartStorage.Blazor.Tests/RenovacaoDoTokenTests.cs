using System.Net;
using System.Net.Http;
using System.Text.Json;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Provider;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// O access token vence em 60 minutos e a sessão dura 9 horas: sem a renovação no 401,
/// o operador volta ao login no meio do turno. A cadeia é a do Program.cs, com o
/// SessionExpiredHandler por fora do RefreshHandler.
/// </summary>
public class RenovacaoDoTokenTests
{
    private const string CaminhoProduto = "api/storage/products/v1/42";
    private const string CaminhoRefresh = "api/auth/v1/refresh";
    private const string CaminhoSignin = "api/auth/v1/signin";

    private readonly LocalStorageFalso storage = new();
    private readonly SessionExpiration sessao = new();
    private readonly RefreshLock trava = new();

    public RenovacaoDoTokenTests()
    {
        storage.Itens[AuthStateProvider.tokenKey] = "token-vencido";
        storage.Itens[AuthStateProvider.refreshTokenKey] = "refresh-atual";
    }

    private HttpClient Cliente(ApiFalsa api, ExigeToken exige, HttpMessageHandler? clienteDoRefresh = null)
    {
        exige.InnerHandler = api;

        var refresh = new RefreshHandler(new FabricaFalsa(clienteDoRefresh ?? api), storage, trava) { InnerHandler = exige };
        var sessaoExpirada = new SessionExpiredHandler(sessao) { InnerHandler = refresh };

        return new HttpClient(new AuthHandler(storage) { InnerHandler = sessaoExpirada })
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }

    private static ApiFalsa ApiComRefreshAceito() => new ApiFalsa()
        .Responde(HttpMethod.Get, CaminhoProduto, new { id = 42 })
        .Responde(HttpMethod.Post, CaminhoRefresh,
            new { authenticated = true, accessToken = "token-novo", refreshToken = "refresh-novo" });

    private static int Refreshes(ApiFalsa api)
        => api.Requisicoes.Count(r => r.Metodo == HttpMethod.Post && r.Caminho == CaminhoRefresh);

    [Fact]
    public async Task Resposta_401_com_refresh_aceito_repete_a_requisicao_com_o_token_novo()
    {
        var api = ApiComRefreshAceito();

        var resposta = await Cliente(api, new ExigeToken("token-novo")).GetAsync(CaminhoProduto);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(new[] { "Bearer token-novo" },
            api.Requisicoes.Select((r, i) => (r, i)).Where(x => x.r.Caminho == CaminhoProduto)
               .Select(x => api.Autorizacoes[x.i]));

        using var corpo = api.JsonDe(HttpMethod.Post, CaminhoRefresh);
        Assert.Equal("token-vencido", corpo.RootElement.GetProperty("accessToken").GetString());
        Assert.Equal("refresh-atual", corpo.RootElement.GetProperty("refreshToken").GetString());

        Assert.Equal("token-novo", storage.Itens[AuthStateProvider.tokenKey]);
        Assert.Equal("refresh-novo", storage.Itens[AuthStateProvider.refreshTokenKey]);
        Assert.False(sessao.IsExpired);
    }

    [Fact]
    public async Task Refresh_recusado_leva_a_sessao_expirada()
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, CaminhoProduto, new { id = 42 })
            .Responde(HttpMethod.Post, CaminhoRefresh, "", HttpStatusCode.Unauthorized);

        var resposta = await Cliente(api, new ExigeToken("token-novo")).GetAsync(CaminhoProduto);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal(1, Refreshes(api));
        Assert.True(sessao.IsExpired);
        Assert.Equal("token-vencido", storage.Itens[AuthStateProvider.tokenKey]);
    }

    [Fact]
    public async Task Dois_401_simultaneos_fazem_um_refresh_so()
    {
        var api = ApiComRefreshAceito();
        var exige = new ExigeToken("token-novo", recusasEsperadas: 2);
        var portao = new Portao { InnerHandler = api };
        var http = Cliente(api, exige, portao);

        var primeira = http.GetAsync(CaminhoProduto);
        var segunda = http.GetAsync(CaminhoProduto);

        // O refresh só sai do portão depois de as duas requisições terem levado o 401.
        await exige.RecusasAtingidas.WaitAsync(TimeSpan.FromSeconds(5));
        portao.Abrir();

        var respostas = await Task.WhenAll(primeira, segunda).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.All(respostas, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        Assert.Equal(1, Refreshes(api));
        Assert.False(sessao.IsExpired);
    }

    [Fact]
    public async Task Senha_errada_no_signin_nao_tenta_o_refresh()
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Post, CaminhoSignin, "", HttpStatusCode.Unauthorized)
            .Responde(HttpMethod.Post, CaminhoRefresh,
                new { authenticated = true, accessToken = "token-novo", refreshToken = "refresh-novo" });

        var resposta = await Cliente(api, new ExigeToken("token-novo")).PostAsync(CaminhoSignin, new StringContent("{}"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal(0, Refreshes(api));
        Assert.False(sessao.IsExpired);
    }

    /// <summary>Responde 401 a qualquer token diferente do válido, como a API com o token vencido.</summary>
    private class ExigeToken : DelegatingHandler
    {
        private readonly string tokenValido;
        private readonly int recusasEsperadas;
        private int recusas;
        private readonly TaskCompletionSource atingidas = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ExigeToken(string tokenValido, int recusasEsperadas = 1)
        {
            this.tokenValido = tokenValido;
            this.recusasEsperadas = recusasEsperadas;
        }

        public Task RecusasAtingidas => atingidas.Task;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/signin")
                || request.Headers.Authorization?.Parameter == tokenValido)
                return base.SendAsync(request, cancellationToken);

            if (Interlocked.Increment(ref recusas) == recusasEsperadas)
                atingidas.TrySetResult();

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        }
    }

    /// <summary>Segura o refresh até o teste abrir, para as duas requisições se encontrarem na trava.</summary>
    private class Portao : DelegatingHandler
    {
        private readonly TaskCompletionSource aberto = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Abrir() => aberto.TrySetResult();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await aberto.Task;
            return await base.SendAsync(request, cancellationToken);
        }
    }

    private class FabricaFalsa(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }
}
