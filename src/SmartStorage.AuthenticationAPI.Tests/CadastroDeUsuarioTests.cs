using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.AuthenticationAPI.Repositories;
using SmartStorage.AuthenticationAPI.Services.Implementations;
using SmartStorage.Shared.Auth;
using SmartStorage.Shared.Enum;
using SmartStorage.Shared.Validation;
using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Tests;

public class CadastroDeUsuarioTests : IClassFixture<ApiEmMemoria>
{
    private const string Repetido = "Já existe um usuário com esse nome.";

    private readonly ApiEmMemoria _api;

    private readonly IUserRepository _usuarios = Substitute.For<IUserRepository>();

    public CadastroDeUsuarioTests(ApiEmMemoria api)
    {
        _api = api;
        _api.Usuarios.FindByUsername("usuario").Returns(new User { Id = 2, Username = "usuario" });
        _api.Usuarios.ClearReceivedCalls();
    }

    private UserAuthServiceImplementation Servico() => new(_usuarios, Substitute.For<IPasswordHasher>());

    private static AccountCredentialsVO Conta(string username) => new()
    {
        Username = username,
        Fullname = "Nome Completo",
        Password = "Senha1234",
        Type = TipoUsuario.Usuario
    };

    [Fact]
    public async Task Cadastro_repetido_responde_400_sem_gravar()
    {
        using var escopo = _api.Services.CreateScope();
        var token = escopo.ServiceProvider.GetRequiredService<ITokenGenerator>().GenerateAccessToken(
        [
            new Claim(JwtRegisteredClaimNames.UniqueName, "admin"),
            new Claim(ClaimTypes.Role, Role.Admin)
        ]);
        using var cliente = _api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/create", Conta("usuario"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(Repetido, await resposta.Content.ReadAsStringAsync());
        _api.Usuarios.DidNotReceive().Create(Arg.Any<User>());
    }

    [Fact]
    public void Indice_unico_violado_na_corrida_vira_recusa_de_regra()
    {
        _usuarios.FindByUsername("novo").Returns((User)null);
        _usuarios.Create(Arg.Any<User>()).Throws(new DbUpdateException("Erro ao salvar.",
            new Exception("Cannot insert duplicate key row in object 'dbo.User' with unique index 'UQ_users_user_name'.")));

        var recusa = Assert.Throws<BusinessException>(() => Servico().Create(Conta("novo")));

        Assert.Equal(Repetido, recusa.Message);
    }

    [Fact]
    public void Outra_falha_de_gravacao_nao_vira_recusa_de_regra()
    {
        _usuarios.FindByUsername("novo").Returns((User)null);
        _usuarios.Create(Arg.Any<User>()).Throws(new DbUpdateException("Erro ao salvar.",
            new Exception("Cannot insert the value NULL into column 'UseFullName'.")));

        Assert.Throws<DbUpdateException>(() => Servico().Create(Conta("novo")));
    }

    [Fact]
    public void Nome_livre_grava()
    {
        _usuarios.FindByUsername("novo").Returns((User)null);
        _usuarios.Create(Arg.Any<User>()).Returns(c => c.Arg<User>());

        var criado = Servico().Create(Conta("novo"));

        Assert.Equal("novo", criado.Username);
        _usuarios.Received(1).Create(Arg.Any<User>());
    }
}
