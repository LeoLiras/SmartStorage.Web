using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Validation;

namespace SmartStorage.Configurations.Tests;

public class ApiExceptionHandlerTests
{
    private sealed class LogFalso : ILogger<ApiExceptionHandler>
    {
        public List<(LogLevel Nivel, Exception Excecao, string Mensagem)> Entradas { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Entradas.Add((logLevel, exception, formatter(state, exception)));
    }

    private static async Task<(HttpContext Contexto, string Corpo, bool Tratou)> Trata(ApiExceptionHandler handler, Exception excecao)
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Method = "POST";
        contexto.Request.Path = "/api/storage/sales/v1";
        contexto.Response.Body = new MemoryStream();

        var tratou = await handler.TryHandleAsync(contexto, excecao, CancellationToken.None);

        contexto.Response.Body.Position = 0;
        var corpo = await new StreamReader(contexto.Response.Body).ReadToEndAsync();

        return (contexto, corpo, tratou);
    }

    [Fact]
    public async Task Recusa_de_regra_responde_400_com_a_mensagem_em_texto_puro()
    {
        var log = new LogFalso();

        var (contexto, corpo, tratou) = await Trata(new ApiExceptionHandler(log), new BusinessException("Saldo insuficiente na prateleira."));

        Assert.True(tratou);
        Assert.Equal(StatusCodes.Status400BadRequest, contexto.Response.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", contexto.Response.ContentType);
        Assert.Equal("Saldo insuficiente na prateleira.", corpo);
        Assert.DoesNotContain(log.Entradas, e => e.Nivel >= LogLevel.Error);
    }

    [Fact]
    public async Task Outra_excecao_responde_500_generico_e_manda_o_detalhe_para_o_log()
    {
        var log = new LogFalso();
        var excecao = new InvalidOperationException("Timeout ao conectar em sqlserver:1433");

        var (contexto, corpo, tratou) = await Trata(new ApiExceptionHandler(log), excecao);

        Assert.True(tratou);
        Assert.Equal(StatusCodes.Status500InternalServerError, contexto.Response.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", contexto.Response.ContentType);
        Assert.Equal("Erro inesperado. Tente novamente.", corpo);
        Assert.DoesNotContain("sqlserver", corpo);

        var entrada = Assert.Single(log.Entradas);
        Assert.Equal(LogLevel.Error, entrada.Nivel);
        Assert.Same(excecao, entrada.Excecao);
        Assert.Contains("POST /api/storage/sales/v1", entrada.Mensagem);
    }
}
