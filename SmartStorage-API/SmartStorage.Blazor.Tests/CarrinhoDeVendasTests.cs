using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Layout;
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Pages.Sale;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Cart;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// Carrinho da issue #21: vive no localStorage do navegador ate a compra ser
/// fechada e vai para a API num unico POST. A recusa da API nao pode esvaziar o
/// carrinho, porque o usuario precisa corrigir o item e tentar de novo.
/// </summary>
public class CarrinhoDeVendasTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Alocacoes = "api/storage/shelf/v1/allocation";
    private const string Lote = "api/storage/sales/v1/batch";
    private const string Chave = "saleCart:admin";

    private static readonly CultureInfo PtBr = new("pt-BR");

    private static object Entrada(int id, int quantidade, decimal preco) => new
    {
        id,
        productId = 40 + id,
        productName = $"Produto {id}",
        productQuantity = quantidade,
        productPrice = preco,
        shelfId = 1,
        shelfName = "Prateleira A1",
        dateEnter = "2026-09-14T10:30:00",
    };

    private static SaleCartItem Item(int id, int quantidade, decimal preco, int disponivel) => new()
    {
        EnterId = id,
        ProductId = 40 + id,
        ProductName = $"Produto {id}",
        ShelfName = "Prateleira A1",
        Price = preco,
        Available = disponivel,
        Qntd = quantidade,
    };

    private ApiFalsa Monta(params SaleCartItem[] salvos)
    {
        var api = new ApiFalsa();

        var dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
               .ReturnsForAnyArgs(Task.FromResult(referencia));

        JSInterop.Mode = JSRuntimeMode.Loose;
        if (salvos.Length > 0)
            JSInterop.Setup<string>("localStorage.getItem", Chave).SetResult(JsonSerializer.Serialize(salvos));

        Services.AddMudServices();
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(dialogo, new SessionExpiration()));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api)
        {
            BaseAddress = new Uri("http://localhost/"),
        }));
        Services.AddSingleton<IProductService>(new ProductService(api.Cliente()));
        Services.AddSingleton<IShelfService>(new ShelfService(api.Cliente()));
        Services.AddScoped<SaleCart>();
        Services.AddSingleton<ISaleService>(new SaleService(api.Cliente()));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> Renderiza<TComponente>() where TComponente : IComponent
        => Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<TComponente>(1);
            builder.CloseComponent();
        });

    private SaleCart Carrinho => Services.GetRequiredService<SaleCart>();

    private List<SaleCartItem> UltimoSalvo()
    {
        var gravacao = JSInterop.Invocations["localStorage.setItem"].Last();

        Assert.Equal(Chave, gravacao.Arguments[0]);

        return JsonSerializer.Deserialize<List<SaleCartItem>>((string)gravacao.Arguments[1]!)!;
    }

    private static string Moeda(decimal valor) => valor.ToString("C", PtBr);

    private static AngleSharp.Dom.IElement Botao(IRenderedComponent<IComponent> cut, string texto)
        => cut.FindAll("button").First(b => b.TextContent.Contains(texto));

    [Fact]
    public void Adicionar_pela_listagem_de_prateleiras_soma_a_mesma_entrada_e_salva_no_navegador()
    {
        var api = Monta();
        api.Responde(HttpMethod.Get, Alocacoes, new[] { Entrada(1, 5, 10.0m) }, total: 1);
        var cut = Renderiza<ProductsShelves>();
        cut.WaitForElement("td button");

        cut.Find("button[aria-label='Adicionar Produto 1 ao carrinho']").Click();
        cut.WaitForAssertion(() => Assert.Equal(1, Assert.Single(Carrinho.Items).Qntd));
        cut.Find("button[aria-label='Adicionar Produto 1 ao carrinho']").Click();
        cut.WaitForAssertion(() => Assert.Equal(2, Assert.Single(Carrinho.Items).Qntd));

        var item = Assert.Single(Carrinho.Items);
        Assert.Equal(1, item.EnterId);
        Assert.Equal(2, item.Qntd);
        Assert.Equal(5, item.Available);

        var salvo = Assert.Single(UltimoSalvo());
        Assert.Equal(2, salvo.Qntd);
        Assert.Equal(10.0m, salvo.Price);

        var aviso = Assert.Single(Services.GetRequiredService<ISnackbar>().ShownSnackbars);
        Assert.Contains("2 unidade(s)", aviso.Message);
    }

    [Fact]
    public void Carrinho_salvo_e_conferido_com_o_saldo_atual_ao_reabrir()
    {
        var api = Monta(Item(1, 4, 10.0m, disponivel: 10));
        api.Responde(HttpMethod.Get, Alocacoes, new[] { Entrada(1, 3, 12.0m) });

        var cut = Renderiza<Cart>();

        cut.WaitForAssertion(() => Assert.Contains("Há 3 na prateleira", cut.Markup));
        Assert.Contains("Há item acima do saldo atual da prateleira", cut.Markup);
        Assert.True(Botao(cut, "Registrar vendas").HasAttribute("disabled"));
        Assert.Contains(Moeda(48.0m), cut.Markup);
        Assert.Equal(3, UltimoSalvo().Single().Available);
    }

    [Fact]
    public void Entrada_que_sumiu_da_api_fica_sem_saldo_no_carrinho()
    {
        var api = Monta(Item(1, 1, 10.0m, disponivel: 10));
        api.Responde(HttpMethod.Get, Alocacoes, Array.Empty<object>());

        var cut = Renderiza<Cart>();

        cut.WaitForAssertion(() => Assert.Contains("Há 0 na prateleira", cut.Markup));
        Assert.True(Botao(cut, "Registrar vendas").HasAttribute("disabled"));
    }

    [Fact]
    public void Alterar_quantidade_e_remover_item_recalculam_o_total()
    {
        var api = Monta(Item(1, 2, 10.0m, disponivel: 10), Item(2, 1, 5.5m, disponivel: 10));
        api.Responde(HttpMethod.Get, Alocacoes, new[] { Entrada(1, 10, 10.0m), Entrada(2, 10, 5.5m) });
        var cut = Renderiza<Cart>();
        cut.WaitForAssertion(() => Assert.Contains(Moeda(25.5m), cut.Markup));

        cut.FindAll("td input")[0].Change("3");

        cut.WaitForAssertion(() => Assert.Contains(Moeda(35.5m), cut.Markup));
        Assert.Equal(3, UltimoSalvo().Single(i => i.EnterId == 1).Qntd);

        cut.Find("button[aria-label='Remover Produto 2 do carrinho']").Click();

        cut.WaitForAssertion(() => Assert.Contains(Moeda(30.0m), cut.Markup));
        Assert.Equal(new[] { 1 }, UltimoSalvo().Select(i => i.EnterId));
    }

    [Fact]
    public void Registrar_envia_um_unico_post_com_todos_os_itens_e_esvazia_o_carrinho()
    {
        var api = Monta(Item(1, 2, 10.0m, disponivel: 10), Item(2, 3, 5.5m, disponivel: 10));
        api.Responde(HttpMethod.Get, Alocacoes, new[] { Entrada(1, 10, 10.0m), Entrada(2, 10, 5.5m) });
        api.Responde(HttpMethod.Post, Lote, new[] { new { id = 1 }, new { id = 2 } });
        var cut = Renderiza<Cart>();
        cut.WaitForAssertion(() => Assert.False(Botao(cut, "Registrar vendas").HasAttribute("disabled")));

        Botao(cut, "Registrar vendas").Click();

        cut.WaitForAssertion(() => Assert.Equal(0, Carrinho.Count));
        Assert.Single(api.Requisicoes, r => r.Metodo == HttpMethod.Post);

        var corpo = api.JsonDe(HttpMethod.Post, Lote).RootElement;
        var itens = corpo.GetProperty("items").EnumerateArray()
            .Select(i => (i.GetProperty("idEnter").GetInt32(), i.GetProperty("qntd").GetInt32()))
            .ToList();

        Assert.Equal(new[] { (1, 2), (2, 3) }, itens);
        Assert.Equal(DateTime.Today, corpo.GetProperty("dateSale").GetDateTime().Date);
        Assert.Contains(JSInterop.Invocations["localStorage.removeItem"], i => (string)i.Arguments[0]! == Chave);
    }

    [Fact]
    public void Recusa_da_api_mantem_o_carrinho_salvo()
    {
        var api = Monta(Item(1, 2, 10.0m, disponivel: 10), Item(2, 3, 5.5m, disponivel: 10));
        api.Responde(HttpMethod.Get, Alocacoes, new[] { Entrada(1, 10, 10.0m), Entrada(2, 10, 5.5m) });
        api.Responde(HttpMethod.Post, Lote, "Item 2 (Produto 2 na Prateleira A1): saldo insuficiente na prateleira: há 1 e o carrinho pede 3.", HttpStatusCode.BadRequest);
        var cut = Renderiza<Cart>();
        cut.WaitForAssertion(() => Assert.False(Botao(cut, "Registrar vendas").HasAttribute("disabled")));

        Botao(cut, "Registrar vendas").Click();

        cut.WaitForAssertion(() => Assert.Equal(2, api.Requisicoes.Count(r => r.Metodo == HttpMethod.Get && r.Caminho == Alocacoes)));
        Assert.Equal(2, Carrinho.Count);
        Assert.Empty(JSInterop.Invocations["localStorage.removeItem"]);
    }

    [Fact]
    public void Limpar_carrinho_esvazia_depois_da_confirmacao()
    {
        var api = Monta(Item(1, 2, 10.0m, disponivel: 10));
        api.Responde(HttpMethod.Get, Alocacoes, new[] { Entrada(1, 10, 10.0m) });
        var cut = Renderiza<Cart>();
        cut.WaitForAssertion(() => Assert.Contains("Limpar carrinho", cut.Markup));

        Botao(cut, "Limpar carrinho").Click();

        cut.WaitForAssertion(() => Assert.Contains("O carrinho está vazio", cut.Markup));
        Assert.Contains(JSInterop.Invocations["localStorage.removeItem"], i => (string)i.Arguments[0]! == Chave);
        Assert.DoesNotContain(api.Requisicoes, r => r.Metodo == HttpMethod.Post);
    }

    [Fact]
    public void Indicador_do_topo_aparece_so_com_itens_no_carrinho()
    {
        Monta(Item(1, 2, 10.0m, disponivel: 10), Item(2, 1, 5.5m, disponivel: 10));

        var cut = Renderiza<CartIndicator>();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find("a[href='products/sales/cart']")));
        Assert.Contains(">2<", cut.Markup.Replace(" ", ""));

        cut.InvokeAsync(() => Carrinho.ClearAsync());

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("a[href='products/sales/cart']")));
    }
}
