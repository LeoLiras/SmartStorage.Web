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
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Tests;

public class MinhaContaTests : IClassFixture<ApiEmMemoria>
{
    private readonly ApiEmMemoria _api;

    private readonly IPasswordHasher _hasher;

    private readonly User _usuario;

    public MinhaContaTests(ApiEmMemoria api)
    {
        _api = api;

        using (var escopo = _api.Services.CreateScope())
            _hasher = escopo.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _usuario = new User
        {
            Id = 2,
            Username = "usuario",
            FullName = "Usuario de Teste",
            Password = _hasher.Hash("usuario123"),
            RefreshToken = "refresh-do-usuario",
            RefreshTokenExpiryTime = new DateTime(2026, 11, 16),
            UseType = TipoUsuario.Usuario
        };

        _api.Usuarios.FindByUsername("usuario").Returns(_usuario);
        _api.Usuarios.Update(Arg.Any<User>()).Returns(c => c.Arg<User>());
        _api.Usuarios.ClearReceivedCalls();
    }

    private HttpClient Cliente()
    {
        using var escopo = _api.Services.CreateScope();

        var token = escopo.ServiceProvider.GetRequiredService<ITokenGenerator>().GenerateAccessToken(
        [
            new Claim(JwtRegisteredClaimNames.UniqueName, "usuario"),
            new Claim(ClaimTypes.Role, Role.Client)
        ]);

        var cliente = _api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    [Fact]
    public async Task Usuario_comum_troca_o_proprio_nome_e_a_propria_senha()
    {
        using var cliente = Cliente();

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/my-account",
            new { fullName = "Usuário Renomeado", currentPassword = "usuario123", newPassword = "NovaSenha1" });
        var json = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Usuário Renomeado", _usuario.FullName);
        Assert.True(_hasher.Verify("NovaSenha1", _usuario.Password));
        _api.Usuarios.Received(1).Update(_usuario);

        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("refreshToken", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Usuário Renomeado", JsonDocument.Parse(json).RootElement.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task Senha_atual_errada_responde_400_sem_gravar()
    {
        using var cliente = Cliente();
        var hashAntes = _usuario.Password;

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/my-account",
            new { fullName = "Usuário Renomeado", currentPassword = "errada", newPassword = "NovaSenha1" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("A senha atual está incorreta.", await resposta.Content.ReadAsStringAsync());
        Assert.Equal("Usuario de Teste", _usuario.FullName);
        Assert.Equal(hashAntes, _usuario.Password);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Corpo_nao_troca_username_papel_nem_refresh_token()
    {
        using var cliente = Cliente();

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/my-account", new
        {
            id = 1,
            username = "admin",
            fullName = "Usuário Renomeado",
            currentPassword = "usuario123",
            type = 1,
            useType = 1,
            refreshToken = "plantado"
        });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(2, _usuario.Id);
        Assert.Equal("usuario", _usuario.Username);
        Assert.Equal(TipoUsuario.Usuario, _usuario.UseType);
        Assert.Equal("refresh-do-usuario", _usuario.RefreshToken);
        _api.Usuarios.DidNotReceive().FindByUsername("admin");
    }

    [Fact]
    public async Task Senha_nova_vazia_muda_so_o_nome()
    {
        using var cliente = Cliente();
        var hashAntes = _usuario.Password;

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/my-account",
            new { fullName = "Usuário Renomeado", currentPassword = "usuario123", newPassword = "  " });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Usuário Renomeado", _usuario.FullName);
        Assert.Equal(hashAntes, _usuario.Password);
    }

    [Fact]
    public async Task Sem_a_senha_atual_responde_400_sem_gravar()
    {
        using var cliente = Cliente();

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/my-account",
            new { fullName = "Usuário Renomeado", newPassword = "NovaSenha1" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }
}
