using System.Net.Http;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Allocation;
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class TransferenciaDePrateleiraTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const int Entrada = 11;
    private const string CaminhoTransferencia = "api/storage/shelf/v1/allocation/11/transfer";

    private IDialogService _dialogo = null!;

    private static object EntradaNaPrateleira(int quantidade) => new
    {
        id = Entrada,
        productId = 42,
        productName = "Capacete de Seguranca Branco",
        productQuantity = quantidade,
        productPrice = 10.0m,
        shelfId = 1,
        shelfName = "Prateleira A1",
        dateEnter = "2026-03-10T00:00:00",
    };

    private ApiFalsa Monta(object entrada)
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, $"api/storage/shelf/v1/allocation/{Entrada}", entrada)
            .Responde(HttpMethod.Get, "api/storage/shelf/v1/allocation", new[] { entrada })
            .Responde(HttpMethod.Post, CaminhoTransferencia, entrada)
            .Responde(HttpMethod.Get, "api/storage/shelf/v1", new[]
            {
                new { id = 1, name = "Prateleira A1" },
                new { id = 2, name = "Prateleira B1" },
            })
            .Responde(HttpMethod.Get, "api/storage/employees/v1", new[]
            {
                new { id = 1, name = "Ana Paula Ribeiro", rg = "MG1234567", cpf = "52998224725" },
            })
            .Responde(HttpMethod.Get, "api/storage/products/v1/42", new
            {
                id = 42,
                name = "Capacete de Seguranca Branco",
                descricao = "Capacete de protecao classe B",
                qntd = 6,
                shelvesQntd = 4,
                employeeId = 1,
                dateRegister = "2026-03-01T00:00:00",
            });

        _dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        _dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
                .ReturnsForAnyArgs(Task.FromResult(referencia));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(_dialogo, new SessionExpiration()));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api)
        {
            BaseAddress = new Uri("http://localhost/"),
        }));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaTransferencia(bool esperaForm = true)
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<ShelfTransfer>(1);
            builder.AddAttribute(2, nameof(ShelfTransfer.enterId), Entrada);
            builder.CloseComponent();
        });

        if (esperaForm)
            cut.WaitForElement("form");

        return cut;
    }

    private static IElement Campo(IRenderedComponent<IComponent> cut, string rotulo)
    {
        var label = cut.FindAll("label").First(l => l.TextContent.Trim() == rotulo);

        return label.Closest(".mud-input-control")!.QuerySelector("input")!;
    }

    [Fact]
    public void Transferir_envia_a_prateleira_de_destino()
    {
        var api = Monta(EntradaNaPrateleira(8));
        var cut = RenderizaTransferencia();

        var destino = cut.FindComponent<MudSelect<int>>();
        cut.InvokeAsync(() => destino.Instance.ValueChanged.InvokeAsync(2));
        cut.Find("form").Submit();

        var escritas = api.Requisicoes.Where(r => r.Metodo != HttpMethod.Get).ToList();
        Assert.Single(escritas);
        Assert.Equal(CaminhoTransferencia, escritas[0].Caminho);
        Assert.Equal(2, api.JsonDe(HttpMethod.Post, CaminhoTransferencia).RootElement.GetProperty("shelfId").GetInt32());
    }

    [Fact]
    public void Tela_de_transferencia_so_deixa_editar_a_prateleira_de_destino()
    {
        Monta(EntradaNaPrateleira(8));
        var cut = RenderizaTransferencia();

        Assert.True(Campo(cut, "Nome do Produto").HasAttribute("readonly"));
        Assert.DoesNotContain("Ajustar estoque do depósito", cut.Markup);
        Assert.True(Campo(cut, "Prateleira atual").HasAttribute("readonly"));
        Assert.Equal("Prateleira A1", Campo(cut, "Prateleira atual").GetAttribute("value"));
        Assert.True(Campo(cut, "Quantidade na prateleira").HasAttribute("readonly"));
        Assert.Equal("8", Campo(cut, "Quantidade na prateleira").GetAttribute("value"));
        Assert.False(cut.FindComponent<MudSelect<int>>().Instance.ReadOnly);
        Assert.Equal("Transferir", cut.Find("button[type=submit]").TextContent.Trim());
    }

    [Fact]
    public void Sem_destino_escolhido_nao_transfere()
    {
        var api = Monta(EntradaNaPrateleira(8));
        var cut = RenderizaTransferencia();

        cut.Find("form").Submit();

        Assert.DoesNotContain(api.Requisicoes, r => r.Metodo != HttpMethod.Get);
        _dialogo.ReceivedWithAnyArgs().ShowAsync<Pages.Dialog.Dialog>(default, default, default);
    }

    [Fact]
    public void Prateleira_sem_saldo_nao_oferece_transferencia()
    {
        var api = Monta(EntradaNaPrateleira(0));
        var cut = RenderizaTransferencia(esperaForm: false);

        Assert.Contains("Não há saldo nesta prateleira para transferir", cut.Markup);
        Assert.Empty(cut.FindAll("button[type=submit]"));
    }

    [Fact]
    public void Listagem_de_prateleiras_leva_a_transferencia()
    {
        Monta(EntradaNaPrateleira(8));

        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<ProductsShelves>(1);
            builder.CloseComponent();
        });

        cut.WaitForElement("td button");
        cut.FindAll("td button")[1].Click();

        Assert.EndsWith($"product/shelf/transfer/{Entrada}", Services.GetRequiredService<NavigationManager>().Uri);
    }
}
