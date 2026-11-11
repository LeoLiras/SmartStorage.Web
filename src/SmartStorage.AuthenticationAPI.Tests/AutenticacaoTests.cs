using System.Net;
using NSubstitute;

namespace SmartStorage.AuthenticationAPI.Tests;

public class AutenticacaoTests : IClassFixture<ApiEmMemoria>
{
    private readonly ApiEmMemoria _api;

    public AutenticacaoTests(ApiEmMemoria api) => _api = api;

    [Fact]
    public async Task Listar_usuarios_sem_token_responde_401_sem_consultar_o_banco()
    {
        using var cliente = _api.CreateClient();

        var resposta = await cliente.GetAsync("/api/auth/v1");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Empty(_api.Usuarios.ReceivedCalls());
    }
}
