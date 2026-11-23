using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.Shared.Auth;
using SmartStorage.Shared.Enum;
using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Tests;

/// <summary>
/// A sessão dura até 9 horas desde o login. O prazo do refresh token era de 7 dias
/// (DaysToExpiry) e passa a ser o fim da sessão, configurado em SessionHours; o refresh
/// troca o refresh token sem estender esse prazo, e o access token não passa dele.
/// </summary>
public class PrazoDaSessaoTests : IClassFixture<ApiEmMemoria>
{
    private readonly ApiEmMemoria _api;

    private readonly User _usuario;

    public PrazoDaSessaoTests(ApiEmMemoria api)
    {
        _api = api;

        IPasswordHasher hasher;
        using (var escopo = _api.Services.CreateScope())
            hasher = escopo.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _usuario = new User
        {
            Id = 4,
            Username = "sessao",
            FullName = "Usuário da Sessão",
            Password = hasher.Hash("sessao@123"),
            UseType = TipoUsuario.Usuario
        };

        _api.Usuarios.FindByUsername("sessao").Returns(_usuario);
        _api.Usuarios.Update(Arg.Any<User>()).Returns(c => c.Arg<User>());
        _api.Usuarios.ClearReceivedCalls();
    }

    private string AccessToken()
    {
        using var escopo = _api.Services.CreateScope();

        return escopo.ServiceProvider.GetRequiredService<ITokenGenerator>().GenerateAccessToken(
        [
            new Claim(JwtRegisteredClaimNames.UniqueName, "sessao"),
            new Claim(ClaimTypes.Role, Role.Client)
        ]);
    }

    private void EmSessaoAte(DateTime fim)
    {
        _usuario.RefreshToken = "refresh-da-sessao";
        _usuario.RefreshTokenExpiryTime = fim;
    }

    private async Task<HttpResponseMessage> Renovar(string? refreshToken = "refresh-da-sessao")
    {
        using var cliente = _api.CreateClient();

        return await cliente.PostAsJsonAsync("/api/auth/v1/refresh", new { accessToken = AccessToken(), refreshToken });
    }

    private static DateTime VencimentoDo(string accessToken)
        => new JwtSecurityTokenHandler().ReadJwtToken(accessToken).ValidTo.ToLocalTime();

    [Fact]
    public async Task Login_grava_o_fim_da_sessao_9_horas_depois()
    {
        using var cliente = _api.CreateClient();

        var antes = DateTime.Now;
        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/signin", new { userName = "sessao", password = "sessao@123" });
        var depois = DateTime.Now;

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(_usuario.RefreshTokenExpiryTime);
        Assert.InRange(_usuario.RefreshTokenExpiryTime!.Value, antes.AddHours(9), depois.AddHours(9));
    }

    [Fact]
    public void Configuracao_compartilhada_tem_SessionHours_e_nao_DaysToExpiry()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);
        while (pasta is not null && !File.Exists(Path.Combine(pasta.FullName, "SmartStorageWeb.sln")))
            pasta = pasta.Parent;

        Assert.NotNull(pasta);

        var arquivo = Path.Combine(pasta!.FullName, "SmartStorage.Configurations", "Json", "appsettings.Shared.json");
        using var json = JsonDocument.Parse(File.ReadAllText(arquivo));
        var token = json.RootElement.GetProperty("TokenConfigurations");

        Assert.Equal(9, token.GetProperty("SessionHours").GetInt32());
        Assert.False(token.TryGetProperty("DaysToExpiry", out _));
    }

    [Fact]
    public async Task Login_emite_access_token_de_60_minutos()
    {
        using var cliente = _api.CreateClient();

        var antes = DateTime.Now;
        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/signin", new { userName = "sessao", password = "sessao@123" });
        var token = await resposta.Content.ReadFromJsonAsync<TokenVO>();

        Assert.InRange(VencimentoDo(token!.AccessToken), antes.AddMinutes(60).AddSeconds(-1), DateTime.Now.AddMinutes(60));
    }

    [Fact]
    public async Task Refresh_troca_o_refresh_token_e_mantem_o_prazo_do_login()
    {
        var fim = DateTime.Now.AddHours(5);
        EmSessaoAte(fim);

        var resposta = await Renovar();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var token = await resposta.Content.ReadFromJsonAsync<TokenVO>();
        Assert.NotEqual("refresh-da-sessao", token!.RefreshToken);
        Assert.Equal(token.RefreshToken, _usuario.RefreshToken);
        Assert.Equal(fim, _usuario.RefreshTokenExpiryTime);
    }

    [Fact]
    public async Task Refresh_depois_do_fim_da_sessao_responde_401()
    {
        EmSessaoAte(DateTime.Now.AddMinutes(-1));

        var resposta = await Renovar();

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Theory]
    [InlineData("refresh-da-sessao")]
    [InlineData(null)]
    public async Task Refresh_depois_do_revoke_responde_401(string? refreshToken)
    {
        EmSessaoAte(DateTime.Now.AddHours(5));

        using (var cliente = _api.CreateClient())
        {
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken());
            var revoke = await cliente.PostAsync("/api/auth/v1/revoke", null);
            Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);
        }

        var resposta = await Renovar(refreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Refresh_a_30_minutos_do_fim_emite_token_que_vence_no_fim_da_sessao()
    {
        var fim = DateTime.Now.AddMinutes(30);
        EmSessaoAte(fim);

        var resposta = await Renovar();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var token = await resposta.Content.ReadFromJsonAsync<TokenVO>();
        Assert.InRange(VencimentoDo(token!.AccessToken), fim.AddSeconds(-1), fim);
        Assert.Equal(fim.ToString("yyyy-MM-dd HH:mm:ss"), token.Expiration);
    }
}
