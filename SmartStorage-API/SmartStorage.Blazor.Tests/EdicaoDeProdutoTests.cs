using System.Net.Http;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class EdicaoDeProdutoTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const int Produto = 42;
    private const int SaldoDeposito = 35;
    private const string Caminho = "api/storage/products/v1/42";

    private IDialogService _dialogo = null!;

    private ApiFalsa Monta()
    {
        var api = new ApiFalsa();

        var produto = new
        {
            id = Produto,
            name = "Capacete de Seguranca Branco",
            descricao = "Capacete de protecao classe B",
            qntd = SaldoDeposito,
            employeeId = 1,
            dateRegister = "2026-03-01T00:00:00",
        };

        api.Responde(HttpMethod.Get, "api/storage/employees/v1", new[]
        {
            new { id = 1, name = "Ana Paula Ribeiro", rg = "MG1234567", cpf = "52998224725" },
        });
        api.Responde(HttpMethod.Get, Caminho, produto);
        api.Responde(HttpMethod.Put, Caminho, produto);

        _dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        _dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
                .ReturnsForAnyArgs(Task.FromResult(referencia));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(_dialogo));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api)
        {
            BaseAddress = new Uri("http://localhost/"),
        }));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> Renderiza()
    {
        return Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<EditProduct>(1);
            builder.AddAttribute(2, nameof(EditProduct.productId), Produto);
            builder.CloseComponent();
        });
    }

    private static IElement Campo(IRenderedComponent<IComponent> cut, string rotulo)
    {
        var label = cut.FindAll("label").First(l => l.TextContent.Trim() == rotulo);

        return label.Closest(".mud-input-control")!.QuerySelector("input")!;
    }

    [Fact]
    public void Atualizar_envia_edicao_e_ajuste_numa_unica_requisicao()
    {
        var api = Monta();
        var cut = Renderiza();

        Campo(cut, "Saldo real conferido").Change("40");
        Campo(cut, "Motivo").Change("Recontagem do inventario");
        cut.Find("form").Submit();

        var escritas = api.Requisicoes.Where(r => r.Metodo != HttpMethod.Get).ToList();
        Assert.Single(escritas);
        Assert.Equal(HttpMethod.Put, escritas[0].Metodo);

        var ajuste = api.JsonDe(HttpMethod.Put, Caminho).RootElement.GetProperty("stockAdjustment");
        Assert.Equal(40, ajuste.GetProperty("quantity").GetInt32());
        Assert.Equal("Recontagem do inventario", ajuste.GetProperty("reason").GetString());
    }

    [Fact]
    public void Atualizar_sem_mudar_o_saldo_nao_envia_ajuste()
    {
        var api = Monta();
        var cut = Renderiza();

        Campo(cut, "Motivo").Change("Motivo digitado sem mudar o saldo");
        cut.Find("form").Submit();

        var corpo = api.JsonDe(HttpMethod.Put, Caminho).RootElement;
        Assert.Equal(System.Text.Json.JsonValueKind.Null, corpo.GetProperty("stockAdjustment").ValueKind);
    }

    [Fact]
    public void Saldo_alterado_sem_motivo_nao_grava_nada()
    {
        var api = Monta();
        var cut = Renderiza();

        Campo(cut, "Saldo real conferido").Change("20");
        cut.Find("form").Submit();

        Assert.DoesNotContain(api.Requisicoes, r => r.Metodo != HttpMethod.Get);
        _dialogo.ReceivedWithAnyArgs().ShowAsync<Pages.Dialog.Dialog>(default, default, default);
    }
}
