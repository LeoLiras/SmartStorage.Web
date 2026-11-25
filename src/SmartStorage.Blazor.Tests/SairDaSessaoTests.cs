using System.Net;
using System.Net.Http;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// Sair só apagava o access token do navegador: o refresh token continuava válido no banco
/// e no localStorage, e renovava a sessão até o fim das 9 horas.
/// </summary>
public class SairDaSessaoTests
{
    private const string CaminhoRevoke = "api/auth/v1/revoke";

    private readonly LocalStorageFalso storage = new();

    public SairDaSessaoTests()
    {
        storage.Itens[AuthStateProvider.tokenKey] = "token-atual";
        storage.Itens[AuthStateProvider.refreshTokenKey] = "refresh-atual";
        storage.Itens["saleCart:admin"] = "[]";
    }

    private async Task<(AuthStateProvider Provedor, bool Notificou)> Sair(HttpMessageHandler api)
    {
        var provedor = new AuthStateProvider(storage);
        var notificou = false;

        provedor.AuthenticationStateChanged += async estado =>
            notificou = !((await estado).User.Identity?.IsAuthenticated ?? false);

        var http = new HttpClient(new AuthHandler(storage) { InnerHandler = api }, disposeHandler: false)
        {
            BaseAddress = new Uri("http://localhost/"),
        };

        await new AuthService(http, provedor, new SessionExpiration()).Logout();

        return (provedor, notificou);
    }

    private async Task AssertSessaoEncerrada(AuthStateProvider provedor, bool notificou)
    {
        Assert.False(storage.Itens.ContainsKey(AuthStateProvider.tokenKey));
        Assert.False(storage.Itens.ContainsKey(AuthStateProvider.refreshTokenKey));
        Assert.True(notificou);
        Assert.False((await provedor.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task Sair_chama_o_revoke_com_o_token_da_sessao_e_limpa_os_tokens()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Post, CaminhoRevoke, "", HttpStatusCode.NoContent);

        var (provedor, notificou) = await Sair(api);

        var indice = api.Requisicoes.FindIndex(r => r.Metodo == HttpMethod.Post && r.Caminho == CaminhoRevoke);
        Assert.True(indice >= 0, "o revoke não foi chamado");
        Assert.Equal("Bearer token-atual", api.Autorizacoes[indice]);

        await AssertSessaoEncerrada(provedor, notificou);
        Assert.True(storage.Itens.ContainsKey("saleCart:admin"));
    }

    [Fact]
    public async Task Revoke_em_500_nao_impede_de_sair()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Post, CaminhoRevoke, "Erro inesperado. Tente novamente.",
            HttpStatusCode.InternalServerError);

        var (provedor, notificou) = await Sair(api);

        await AssertSessaoEncerrada(provedor, notificou);
    }

    [Fact]
    public async Task Sem_rede_o_navegador_sai_do_mesmo_jeito()
    {
        var (provedor, notificou) = await Sair(new SemRede());

        await AssertSessaoEncerrada(provedor, notificou);
    }

    private class SemRede : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("TypeError: Failed to fetch");
    }
}
