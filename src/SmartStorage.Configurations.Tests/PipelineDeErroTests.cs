using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Config;
using SmartStorage.Shared.Validation;

namespace SmartStorage.Configurations.Tests;

public class PipelineDeErroTests : IAsyncLifetime
{
    private WebApplication _app;

    private HttpClient _cliente;

    public async Task InitializeAsync()
    {
        // O mesmo pipeline dos cinco serviços: o UseApiDefaults com o tratador registrado como nos Program.cs.
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddHealthChecks();
        builder.Services.AddSwagger("Teste", "Teste", "v1");
        builder.Services.AddAuthentication();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();

        _app = builder.Build();
        _app.UseApiDefaults("Teste", "v1");
        _app.MapGet("/recusa", string () => throw new BusinessException("Saldo insuficiente na prateleira."));
        _app.MapGet("/falha", string () => throw new InvalidOperationException("Timeout ao conectar em sqlserver:1433"));

        await _app.StartAsync();
        _cliente = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _cliente.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Recusa_de_regra_responde_400_com_o_motivo_em_texto_puro()
    {
        var resposta = await _cliente.GetAsync("/recusa");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("text/plain", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Saldo insuficiente na prateleira.", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Erro_inesperado_responde_500_generico_sem_o_texto_interno()
    {
        var resposta = await _cliente.GetAsync("/falha");

        var corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal(ApiExceptionHandler.UnexpectedErrorMessage, corpo);
        Assert.DoesNotContain("sqlserver", corpo);
    }
}
