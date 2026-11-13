using System.Net;
using System.Net.Http.Json;
using NSubstitute;

namespace SmartStorage.AuthenticationAPI.Tests;

// Classe própria, com a sua instância da API: o falso de usuários aqui lança, e o AutenticacaoTests confere que ele não é chamado.
public class TratamentoDeErrosTests : IClassFixture<ApiEmMemoria>
{
    private readonly ApiEmMemoria _api;

    public TratamentoDeErrosTests(ApiEmMemoria api) => _api = api;

    [Fact]
    public async Task Falha_do_banco_no_login_responde_500_generico_sem_o_texto_interno()
    {
        _api.Usuarios.FindByUsername("admin").Returns(_ => throw new InvalidOperationException("Timeout ao conectar em sqlserver:1433"));

        using var cliente = _api.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/signin", new { userName = "admin", password = "admin123" });

        var corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("Erro inesperado. Tente novamente.", corpo);
        Assert.DoesNotContain("sqlserver", corpo);
    }
}
