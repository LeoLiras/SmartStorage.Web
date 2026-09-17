using System.Net;
using System.Net.Http;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Utils.API;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// EmployeeService da issue #22: ultimo dominio a deixar o ApiExtensions, que
/// saiu do Blazor depois dele.
/// </summary>
public class ServicoDeFuncionariosTests
{
    private const string Funcionarios = "api/storage/employees/v1";

    [Fact]
    public async Task Lista_os_funcionarios_da_api()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, Funcionarios, new[]
        {
            new { id = 1, name = "Ana Paula Ribeiro" },
            new { id = 2, name = "Bruno Costa" },
        });
        var servico = new EmployeeService(api.Cliente());

        var funcionarios = await servico.GetEmployees();

        Assert.Equal(new[] { 1, 2 }, funcionarios.Select(f => f.Id));
        Assert.Single(api.Requisicoes, r => r.Metodo == HttpMethod.Get && r.Caminho == Funcionarios);
    }

    [Fact]
    public async Task Resposta_de_erro_vira_api_exception_com_a_mensagem_da_api()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, Funcionarios, "Falha ao buscar funcionários.", HttpStatusCode.InternalServerError);
        var servico = new EmployeeService(api.Cliente());

        var erro = await Assert.ThrowsAsync<ApiException>(() => servico.GetEmployees());

        Assert.Equal(500, erro.StatusCode);
        Assert.Equal("Falha ao buscar funcionários.", erro.Message);
    }
}
