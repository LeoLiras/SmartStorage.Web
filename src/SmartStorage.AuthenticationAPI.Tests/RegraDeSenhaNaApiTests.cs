using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.Shared.Auth;
using SmartStorage.Shared.Enum;
using SmartStorage.Shared.Validation;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Tests;

/// <summary>
/// A regra da PasswordPolicy vale nos três pontos onde a senha nasce ou muda, e a
/// senha nova igual à atual é recusada nas duas trocas, mesmo que a tela tenha sido contornada.
/// </summary>
public class RegraDeSenhaNaApiTests : IClassFixture<ApiEmMemoria>
{
    private const string SenhaAtual = "Atual@123";

    private const string SamePasswordMessage = "A nova senha precisa ser diferente da atual.";

    private readonly ApiEmMemoria _api;

    private readonly User _usuario;

    private readonly User _maria;

    public RegraDeSenhaNaApiTests(ApiEmMemoria api)
    {
        _api = api;

        IPasswordHasher hasher;
        using (var escopo = _api.Services.CreateScope())
            hasher = escopo.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _usuario = new User { Id = 2, Username = "usuario", FullName = "Usuário Comum", Password = hasher.Hash(SenhaAtual), UseType = TipoUsuario.Usuario };
        _maria = new User { Id = 3, Username = "maria", FullName = "Maria Souza", Password = hasher.Hash(SenhaAtual), UseType = TipoUsuario.Usuario };

        _api.Usuarios.FindByUsername("usuario").Returns(_usuario);
        _api.Usuarios.FindByUsername("novo.usuario").Returns((User)null);
        _api.Usuarios.FindById(3).Returns(_maria);
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

    [Theory]
    [InlineData("Curta@1")]
    [InlineData("SemNumero!")]
    [InlineData("SemEspecial1")]
    public async Task Cadastro_com_senha_fora_da_regra_responde_400_sem_gravar(string senha)
    {
        using var cliente = Cliente("admin", Role.Admin);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/create",
            new { username = "novo.usuario", fullname = "Novo Usuário", password = senha, type = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(PasswordPolicy.RuleMessage, await resposta.Content.ReadAsStringAsync());
        _api.Usuarios.DidNotReceive().Create(Arg.Any<User>());
    }

    [Fact]
    public async Task Minha_conta_com_senha_nova_fora_da_regra_responde_400_sem_gravar()
    {
        using var cliente = Cliente("usuario", Role.Client);

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/my-account",
            new { fullName = "Usuário Renomeado", currentPassword = SenhaAtual, newPassword = "SemEspecial1" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(PasswordPolicy.RuleMessage, await resposta.Content.ReadAsStringAsync());
        Assert.Equal("Usuário Comum", _usuario.FullName);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Minha_conta_com_senha_nova_igual_a_atual_responde_400_sem_gravar()
    {
        using var cliente = Cliente("usuario", Role.Client);

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/my-account",
            new { fullName = "Usuário Renomeado", currentPassword = SenhaAtual, newPassword = SenhaAtual });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(SamePasswordMessage, await resposta.Content.ReadAsStringAsync());
        Assert.Equal("Usuário Comum", _usuario.FullName);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Rota_do_administrador_com_senha_fora_da_regra_responde_400_sem_gravar()
    {
        using var cliente = Cliente("admin", Role.Admin);
        var hashAntes = _maria.Password;

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/3/password", new { newPassword = "SemEspecial1" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(PasswordPolicy.RuleMessage, await resposta.Content.ReadAsStringAsync());
        Assert.Equal(hashAntes, _maria.Password);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Rota_do_administrador_com_senha_igual_a_atual_responde_400_sem_gravar()
    {
        using var cliente = Cliente("admin", Role.Admin);
        var hashAntes = _maria.Password;

        var resposta = await cliente.PutAsJsonAsync("/api/auth/v1/3/password", new { newPassword = SenhaAtual });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(SamePasswordMessage, await resposta.Content.ReadAsStringAsync());
        Assert.Equal(hashAntes, _maria.Password);
        _api.Usuarios.DidNotReceive().Update(Arg.Any<User>());
    }
}
