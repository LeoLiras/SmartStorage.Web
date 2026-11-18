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

public class SenhaDoUsuarioTests : IClassFixture<ApiEmMemoria>
{
    private readonly ApiEmMemoria _api;

    private readonly IPasswordHasher _hasher;

    private readonly User _maria;

    private readonly User _admin;

    public SenhaDoUsuarioTests(ApiEmMemoria api)
    {
        _api = api;

        using (var escopo = _api.Services.CreateScope())
            _hasher = escopo.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _maria = new User
        {
            Id = 3,
            Username = "maria",
            FullName = "Maria Souza",
            Password = _hasher.Hash("senha-da-maria"),
            RefreshToken = "refresh-da-maria",
            UseType = TipoUsuario.Usuario
        };

        _admin = new User
        {
            Id = 1,
            Username = "admin",
            FullName = "Administrador",
            Password = _hasher.Hash("admin123"),
            UseType = TipoUsuario.Administrador
        };

        _api.Usuarios.FindById(3).Returns(_maria);
        _api.Usuarios.FindById(1).Returns(_admin);
        _api.Usuarios.FindById(99).Returns((User)null);
        _api.Usuarios.Update(Arg.Any<User>()).Returns(c => c.Arg<User>());
        _api.Usuarios.ClearReceivedCalls();
    }

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

    [Fact]
    public async Task Administrador_define_a_senha_de_outro_usuario()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/3/password", new { newPassword = "NovaSenha@1" });
        var json = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.True(_hasher.Verify("NovaSenha@1", _maria.Password));
        _api.Usuarios.Received(1).Update(_maria);

        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("refreshToken", json, StringComparison.OrdinalIgnoreCase);
        var conta = JsonDocument.Parse(json).RootElement;
        Assert.Equal(3, conta.GetProperty("id").GetInt64());
        Assert.Equal("maria", conta.GetProperty("username").GetString());
    }

    [Fact]
    public async Task Usuario_comum_recebe_403_sem_gravar()
    {
        using var cliente = Cliente("usuario", Role.Client);

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/3/password", new { newPassword = "NovaSenha@1" });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Empty(_api.Usuarios.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Senha_nova_vazia_responde_400_sem_gravar(string senha)
    {
        using var cliente = Cliente("admin", Role.Admin);
        var hashAntes = _maria.Password;

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/3/password", new { newPassword = senha });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("A nova senha é obrigatória.", await resposta.Content.ReadAsStringAsync());
        Assert.Equal(hashAntes, _maria.Password);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Administrador_nao_define_a_propria_senha_por_aqui()
    {
        using var cliente = Cliente("admin", Role.Admin);
        var hashAntes = _admin.Password;

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/1/password", new { newPassword = "NovaSenha@1" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Para alterar a própria senha, use a sua conta.", await resposta.Content.ReadAsStringAsync());
        Assert.Equal(hashAntes, _admin.Password);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Usuario_inexistente_responde_400_sem_gravar()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/99/password", new { newPassword = "NovaSenha@1" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Usuário não encontrado com o ID informado", await resposta.Content.ReadAsStringAsync());
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }
}
