using System.Net;
using System.Net.Http;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Allocation;
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Utils.Cart;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class InventarioDePrateleiraTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string CaminhoInventario = "api/storage/shelf/v1/1/inventory";

    private IDialogService _dialogo = null!;

    private static object Entrada(int id, int produto, string nome, int quantidade, int prateleira) => new
    {
        id,
        productId = produto,
        productName = nome,
        productQuantity = quantidade,
        productPrice = 10.0m,
        shelfId = prateleira,
        shelfName = prateleira == 1 ? "Prateleira A1" : "Prateleira B1",
        dateEnter = "2026-03-10T00:00:00",
    };

    private ApiFalsa Monta(HttpStatusCode statusInventario = HttpStatusCode.OK)
    {
        var entradas = new[]
        {
            Entrada(11, 42, "Luva Nitrilica", 8, 1),
            Entrada(12, 43, "Capacete Branco", 5, 1),
            Entrada(13, 44, "Oculos Zerado", 0, 1),
            Entrada(14, 45, "Bota de Outra Prateleira", 3, 2),
        };

        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, "api/storage/shelf/v1", new[]
            {
                new { id = 1, name = "Prateleira A1" },
                new { id = 2, name = "Prateleira B1" },
            })
            .Responde(HttpMethod.Get, "api/storage/shelf/v1/allocation", entradas);

        if (statusInventario == HttpStatusCode.OK)
            api.Responde(HttpMethod.Post, CaminhoInventario, new[] { entradas[0] });
        else
            api.Responde(HttpMethod.Post, CaminhoInventario, "Item 1 (Luva Nitrilica): entrada não encontrada com o ID informado.", statusInventario);

        _dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        _dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
                .ReturnsForAnyArgs(Task.FromResult(referencia));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(_dialogo, new SessionExpiration()));
        Services.AddSingleton<IProductService>(new ProductService(api.Cliente()));
        Services.AddSingleton<IShelfService>(new ShelfService(api.Cliente()));
        Services.AddScoped<SaleCart>();
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaInventario()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<ShelfInventory>(1);
            builder.CloseComponent();
        });

        cut.WaitForAssertion(() => cut.FindComponent<MudSelect<int>>(), TimeSpan.FromSeconds(5));

        return cut;
    }

    private static void EscolhePrateleira(IRenderedComponent<IComponent> cut, int prateleira)
    {
        var select = cut.FindComponent<MudSelect<int>>();
        cut.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(prateleira));
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("tbody tr")), TimeSpan.FromSeconds(5));
    }

    private static void Conta(IRenderedComponent<IComponent> cut, int linha, int quantidade)
    {
        var campo = cut.FindComponents<MudNumericField<int>>()[linha];
        cut.InvokeAsync(() => campo.Instance.ValueChanged.InvokeAsync(quantidade));
    }

    private static void InformaMotivo(IRenderedComponent<IComponent> cut, string motivo)
    {
        var campo = cut.FindComponent<MudTextField<string>>();
        cut.InvokeAsync(() => campo.Instance.ValueChanged.InvokeAsync(motivo));
    }

    private static IElement BotaoRegistrar(IRenderedComponent<IComponent> cut)
        => cut.FindAll("button").First(b => b.TextContent.Trim() == "Registrar inventário");

    [Fact]
    public void Escolher_a_prateleira_carrega_os_produtos_com_saldo_dela()
    {
        Monta();
        var cut = RenderizaInventario();

        EscolhePrateleira(cut, 1);

        var linhas = cut.FindAll("tbody tr");
        Assert.Equal(2, linhas.Count);
        Assert.Contains("Capacete Branco", linhas[0].TextContent);
        Assert.Contains("Luva Nitrilica", linhas[1].TextContent);
        Assert.DoesNotContain("Oculos Zerado", cut.Markup);
        Assert.DoesNotContain("Bota de Outra Prateleira", cut.Markup);
        Assert.Equal(new[] { 5, 8 }, cut.FindComponents<MudNumericField<int>>().Select(c => c.Instance.Value));
    }

    [Fact]
    public void Registrar_envia_so_as_quantidades_alteradas_com_o_motivo()
    {
        var api = Monta();
        var cut = RenderizaInventario();
        EscolhePrateleira(cut, 1);

        Conta(cut, 1, 6);
        InformaMotivo(cut, "Contagem mensal");
        cut.WaitForAssertion(() => Assert.False(BotaoRegistrar(cut).HasAttribute("disabled")), TimeSpan.FromSeconds(5));
        cut.WaitForAssertion(() => Assert.Contains("-2", cut.FindAll("tbody tr")[1].TextContent), TimeSpan.FromSeconds(5));
        Assert.Contains("1 produto(s) com diferença", cut.Markup);

        BotaoRegistrar(cut).Click();

        cut.WaitForAssertion(() => Assert.Contains(api.Requisicoes, r => r.Metodo == HttpMethod.Post), TimeSpan.FromSeconds(5));
        var escritas = api.Requisicoes.Where(r => r.Metodo != HttpMethod.Get).ToList();
        Assert.Single(escritas);
        Assert.Equal(CaminhoInventario, escritas[0].Caminho);
        var corpo = api.JsonDe(HttpMethod.Post, CaminhoInventario).RootElement;
        Assert.Equal("Contagem mensal", corpo.GetProperty("reason").GetString());
        var itens = corpo.GetProperty("items").EnumerateArray().ToList();
        Assert.Single(itens);
        Assert.Equal(11, itens[0].GetProperty("enterId").GetInt32());
        Assert.Equal(6, itens[0].GetProperty("countedQntd").GetInt32());
    }

    [Fact]
    public void Sem_diferenca_ou_sem_motivo_nao_registra()
    {
        var api = Monta();
        var cut = RenderizaInventario();
        EscolhePrateleira(cut, 1);

        InformaMotivo(cut, "Contagem mensal");
        cut.WaitForAssertion(() => Assert.True(BotaoRegistrar(cut).HasAttribute("disabled")), TimeSpan.FromSeconds(5));

        Conta(cut, 0, 7);
        InformaMotivo(cut, "abc");
        cut.WaitForAssertion(() => Assert.Contains("pelo menos 5 caracteres", cut.Markup), TimeSpan.FromSeconds(5));
        Assert.True(BotaoRegistrar(cut).HasAttribute("disabled"));

        Assert.DoesNotContain(api.Requisicoes, r => r.Metodo != HttpMethod.Get);
    }

    [Fact]
    public void Recusa_da_API_avisa_e_mantem_a_contagem()
    {
        var api = Monta(HttpStatusCode.BadRequest);
        var cut = RenderizaInventario();
        EscolhePrateleira(cut, 1);

        Conta(cut, 1, 6);
        InformaMotivo(cut, "Contagem mensal");
        cut.WaitForAssertion(() => Assert.False(BotaoRegistrar(cut).HasAttribute("disabled")), TimeSpan.FromSeconds(5));

        BotaoRegistrar(cut).Click();

        cut.WaitForAssertion(() => _dialogo.ReceivedWithAnyArgs().ShowAsync<Pages.Dialog.Dialog>(default, default, default), TimeSpan.FromSeconds(5));
        cut.WaitForAssertion(() => Assert.Equal(6, cut.FindComponents<MudNumericField<int>>()[1].Instance.Value), TimeSpan.FromSeconds(5));
        Assert.Equal("Contagem mensal", cut.FindComponent<MudTextField<string>>().Instance.Value);
    }

    [Fact]
    public void Listagem_de_prateleiras_leva_ao_inventario()
    {
        var api = Monta();
        api.Responde(HttpMethod.Get, "api/storage/shelf/v1/allocation", Array.Empty<object>(), total: 0);

        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<ProductsShelves>(1);
            builder.CloseComponent();
        });

        var botao = cut.WaitForElement("a[href='products/shelves/inventory']");
        Assert.Contains("Inventário", botao.TextContent);
    }
}
