using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.Shared.Auth;
using SmartStorage.Shared.Enum;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Tests;

public class LeituraDeUsuariosTests : IClassFixture<ApiEmMemoria>
{
    private readonly ApiEmMemoria _api;

    public LeituraDeUsuariosTests(ApiEmMemoria api)
    {
        _api = api;
        _api.Usuarios.FindAll().Returns([Usuario(1, "admin", TipoUsuario.Administrador), Usuario(2, "usuario", TipoUsuario.Usuario)]);
        _api.Usuarios.FindById(2).Returns(Usuario(2, "usuario", TipoUsuario.Usuario));
        _api.Usuarios.FindByUsername("usuario").Returns(Usuario(2, "usuario", TipoUsuario.Usuario));
        _api.Usuarios.ClearReceivedCalls();
    }

    private static User Usuario(long id, string username, TipoUsuario tipo) => new()
    {
        Id = id,
        Username = username,
        FullName = $"Nome de {username}",
        Password = $"hash-de-{username}",
        RefreshToken = $"refresh-de-{username}",
        RefreshTokenExpiryTime = new DateTime(2026, 11, 16),
        UseType = tipo
    };

    private HttpClient Cliente(string username, string papel)
    {
        using var escopo = _api.Services.CreateScope();

        var token = escopo.ServiceProvider.GetRequiredService<ITokenGenerator>().GenerateAccessToken(
        [
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, papel)
        ]);

        var cliente = _api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    private static void SemSegredos(string json)
    {
        Assert.DoesNotContain("hash-de-", json);
        Assert.DoesNotContain("refresh-de-", json);
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("refreshToken", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Administrador_lista_os_usuarios_sem_senha_nem_refresh_token()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.GetAsync("/api/auth/v1");
        var json = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        SemSegredos(json);

        var usuarios = JsonDocument.Parse(json).RootElement;
        Assert.Equal(2, usuarios.GetArrayLength());
        Assert.Equal("admin", usuarios[0].GetProperty("username").GetString());
        Assert.Equal("Nome de admin", usuarios[0].GetProperty("fullName").GetString());
        Assert.Equal((int)TipoUsuario.Administrador, usuarios[0].GetProperty("type").GetInt32());
    }

    [Fact]
    public async Task Administrador_busca_usuario_por_id_sem_senha_nem_refresh_token()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.GetAsync("/api/auth/v1/2");
        var json = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        SemSegredos(json);
        Assert.Equal(2, JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt64());
    }

    [Theory]
    [InlineData("/api/auth/v1")]
    [InlineData("/api/auth/v1/2")]
    public async Task Usuario_comum_recebe_403_ao_listar_ou_buscar_por_id(string caminho)
    {
        using var cliente = Cliente("usuario", Role.Client);

        var resposta = await cliente.GetAsync(caminho);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Empty(_api.Usuarios.ReceivedCalls());
    }

    [Fact]
    public async Task Usuario_comum_le_a_propria_conta_sem_senha_nem_refresh_token()
    {
        using var cliente = Cliente("usuario", Role.Client);

        var resposta = await cliente.GetAsync("/api/auth/v1/my-account");
        var json = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        SemSegredos(json);

        var conta = JsonDocument.Parse(json).RootElement;
        Assert.Equal(2, conta.GetProperty("id").GetInt64());
        Assert.Equal("usuario", conta.GetProperty("username").GetString());
        Assert.Equal((int)TipoUsuario.Usuario, conta.GetProperty("type").GetInt32());
    }

    [Fact]
    public async Task Minha_conta_ignora_username_na_query_e_le_o_do_token()
    {
        using var cliente = Cliente("usuario", Role.Client);

        var resposta = await cliente.GetAsync("/api/auth/v1/my-account?userName=admin");
        var json = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("usuario", JsonDocument.Parse(json).RootElement.GetProperty("username").GetString());
        _api.Usuarios.DidNotReceive().FindByUsername("admin");
    }

    [Fact]
    public async Task Busca_por_username_nao_existe_mais()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.GetAsync("/api/auth/v1/user-by-username?userName=usuario");

        Assert.NotEqual(HttpStatusCode.OK, resposta.StatusCode);
        _api.Usuarios.DidNotReceive().FindByUsername("usuario");
    }
}
