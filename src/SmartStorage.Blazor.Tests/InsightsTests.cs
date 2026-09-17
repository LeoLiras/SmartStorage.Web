using System.Net;
using System.Net.Http;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Insights;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class InsightsTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private IDialogService _dialogo = null!;

    private ApiFalsa Monta()
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, "api/storage/sales/v1", new[]
            {
                new
                {
                    id = 1,
                    idEnter = 3,
                    productId = 42,
                    productName = "Capacete de Seguranca Branco",
                    shelfName = "Prateleira B1",
                    salePrice = 10.0m,
                    qntd = 3,
                    returnedQntd = 0,
                    saleTotal = 30.0m,
                    dateSale = DateTime.Now.ToString("s"),
                },
            });

        _dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        _dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
                .ReturnsForAnyArgs(Task.FromResult(referencia));

        var http = new HttpClient(api) { BaseAddress = new Uri("http://localhost/") };

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(_dialogo, new SessionExpiration()));
        Services.AddSingleton<ISaleService>(new SaleService(http));
        Services.AddSingleton<IReportsService>(new ReportsService(http));
        Services.AddSingleton<IAiService>(new AiService(http));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaInsights()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<Insights>(1);
            builder.CloseComponent();
        });

        cut.WaitForAssertion(() => Assert.Contains("Olá, como posso ajudar?", cut.Markup));

        return cut;
    }

    private List<string> TitulosDosDialogos()
        => _dialogo.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IDialogService.ShowAsync))
            .Select(c => (string)c.GetArguments()[0]!)
            .ToList();

    [Theory]
    [InlineData("Excel", "api/storage/reports/v1/export-excel")]
    [InlineData("PDF", "api/storage/reports/v1/export-pdf")]
    public void Erro_no_relatorio_mostra_o_aviso_e_para_o_carregamento(string botao, string caminho)
    {
        Monta().Responde(HttpMethod.Get, caminho, "Ainda não há vendas no mês corrente.", HttpStatusCode.BadRequest);
        var cut = RenderizaInsights();

        cut.FindAll("button").First(b => b.TextContent.Trim() == botao).Click();

        cut.WaitForAssertion(() => Assert.Equal(new[] { "Erro" }, TitulosDosDialogos()));
        Assert.Empty(cut.FindAll(".app-busy"));
        Assert.DoesNotContain(JSInterop.Invocations, i => i.Identifier == "downloadFile");
    }

    [Fact]
    public void Relatorio_gerado_e_baixado_sem_aviso()
    {
        Monta().Responde(HttpMethod.Get, "api/storage/reports/v1/export-excel", "planilha");
        var cut = RenderizaInsights();

        cut.FindAll("button").First(b => b.TextContent.Trim() == "Excel").Click();

        cut.WaitForAssertion(() => Assert.Contains(JSInterop.Invocations, i => i.Identifier == "downloadFile"));
        Assert.Empty(TitulosDosDialogos());
        Assert.Empty(cut.FindAll(".app-busy"));
    }

    [Fact]
    public void Erro_da_ia_nao_aparece_como_resposta_no_chat()
    {
        Monta().Responde(HttpMethod.Post, "api/storage/ai/v1/analyse-sales", "Chave do Gemini ausente", HttpStatusCode.BadRequest);
        var cut = RenderizaInsights();

        cut.Find("input").Change("Qual produto vendeu mais?");
        cut.FindComponent<MudIconButton>().Find("button").Click();

        cut.WaitForAssertion(() => Assert.Equal(new[] { "Erro" }, TitulosDosDialogos()));
        Assert.DoesNotContain("Chave do Gemini ausente", cut.Markup);
        Assert.Empty(cut.FindAll(".app-busy"));
    }

    [Fact]
    public void Resposta_da_ia_aparece_no_chat()
    {
        Monta().Responde(HttpMethod.Post, "api/storage/ai/v1/analyse-sales", "O capacete foi o mais vendido.");
        var cut = RenderizaInsights();

        cut.Find("input").Change("Qual produto vendeu mais?");
        cut.FindComponent<MudIconButton>().Find("button").Click();

        cut.WaitForAssertion(() => Assert.Contains("O capacete foi o mais vendido.", cut.Markup));
        Assert.Empty(TitulosDosDialogos());
    }
}
