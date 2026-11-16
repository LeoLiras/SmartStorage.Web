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

public class EdicaoDeUsuarioTests : IClassFixture<ApiEmMemoria>
{
    private static readonly DateTime Expira = new(2026, 11, 16);

    private readonly ApiEmMemoria _api;

    private readonly User _maria = new()
    {
        Id = 3,
        Username = "maria",
        FullName = "Maria Souza",
        Password = "hash-da-maria",
        RefreshToken = "refresh-da-maria",
        RefreshTokenExpiryTime = Expira,
        UseType = TipoUsuario.Usuario
    };

    public EdicaoDeUsuarioTests(ApiEmMemoria api)
    {
        _api = api;
        _api.Usuarios.FindById(3).Returns(_maria);
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
    public async Task Corpo_com_campos_fora_do_VO_so_muda_nome_e_papel()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/update-credentials/3", new
        {
            id = 1,
            username = "admin",
            fullName = "Maria Souza Lima",
            type = 1,
            useType = 0,
            password = "NovaSenha123",
            refreshToken = "plantado",
            refreshTokenExpiryTime = new DateTime(2030, 1, 1)
        });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        Assert.Equal(3, _maria.Id);
        Assert.Equal("maria", _maria.Username);
        Assert.Equal("Maria Souza Lima", _maria.FullName);
        Assert.Equal(TipoUsuario.Administrador, _maria.UseType);
        Assert.Equal("hash-da-maria", _maria.Password);
        Assert.Equal("refresh-da-maria", _maria.RefreshToken);
        Assert.Equal(Expira, _maria.RefreshTokenExpiryTime);
        _api.Usuarios.Received(1).Update(_maria);
    }

    [Fact]
    public async Task Resposta_traz_a_conta_sem_senha_nem_refresh_token()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/update-credentials/3", new { fullName = "Maria Souza", type = 0 });
        var json = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.DoesNotContain("hash-da-maria", json);
        Assert.DoesNotContain("refresh-da-maria", json);
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

        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/update-credentials/3", new { fullName = "Maria", type = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Empty(_api.Usuarios.ReceivedCalls());
    }

    [Fact]
    public async Task Usuario_inexistente_responde_400_sem_gravar()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/update-credentials/99", new { fullName = "Ninguém", type = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Usuário não encontrado com o ID informado", await resposta.Content.ReadAsStringAsync());
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Nome_completo_vazio_responde_400_sem_gravar()
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/update-credentials/3", new { fullName = "", type = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }
}
