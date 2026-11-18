using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.Shared.Auth;
using SmartStorage.Shared.Enum;
using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Tests;

public class RefreshTests : IClassFixture<ApiEmMemoria>
{
    private readonly ApiEmMemoria _api;

    private readonly User _usuario;

    public RefreshTests(ApiEmMemoria api)
    {
        _api = api;

        _usuario = new User
        {
            Id = 3,
            Username = "rebaixado",
            FullName = "Usuário Rebaixado",
            RefreshToken = "refresh-do-rebaixado",
            RefreshTokenExpiryTime = DateTime.Now.AddDays(1),
            UseType = TipoUsuario.Usuario
        };

        _api.Usuarios.FindByUsername("rebaixado").Returns(_usuario);
        _api.Usuarios.Update(Arg.Any<User>()).Returns(c => c.Arg<User>());
        _api.Usuarios.ClearReceivedCalls();
    }

    private string TokenDeAdministrador()
    {
        using var escopo = _api.Services.CreateScope();

        return escopo.ServiceProvider.GetRequiredService<ITokenGenerator>().GenerateAccessToken(
        [
            new Claim(JwtRegisteredClaimNames.UniqueName, "rebaixado"),
            new Claim(ClaimTypes.Role, Role.Admin)
        ]);
    }

    private async Task<HttpResponseMessage> Renovar(string accessToken)
    {
        using var cliente = _api.CreateClient();

        return await cliente.PostAsJsonAsync("/api/auth/v1/refresh",
            new { accessToken, refreshToken = "refresh-do-rebaixado" });
    }

    [Fact]
    public async Task Refresh_depois_de_rebaixar_o_papel_emite_Usuario()
    {
        var resposta = await Renovar(TokenDeAdministrador());

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var token = await resposta.Content.ReadFromJsonAsync<TokenVO>();
        var papeis = new JwtSecurityTokenHandler().ReadJwtToken(token!.AccessToken).Claims
            .Where(c => c.Type is ClaimTypes.Role or "role")
            .Select(c => c.Value);

        Assert.Equal([Role.Client], papeis);
        Assert.NotEqual("refresh-do-rebaixado", token.RefreshToken);
    }

    [Fact]
    public async Task Token_adulterado_responde_401_sem_renovar()
    {
        var partes = TokenDeAdministrador().Split('.');
        var assinatura = partes[2];
        partes[2] = (assinatura[0] == 'A' ? 'B' : 'A') + assinatura[1..];

        var resposta = await Renovar(string.Join('.', partes));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Texto_que_nao_e_JWT_responde_401()
    {
        var resposta = await Renovar("isto-nao-e-um-jwt");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }
}
