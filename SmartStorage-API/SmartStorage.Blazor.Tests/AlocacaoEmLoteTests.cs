using System.Net;
using System.Net.Http;
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
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// Alocacao em lote da issue #20: os cards marcados viram uma tela com um item
/// por produto, quantidade 1, o preco inicial do produto e a prateleira onde ele
/// ja esta, e tudo vai num unico POST. A tela avisa teto somado e segunda
/// prateleira, mas quem recusa e a API.
/// </summary>
public class AlocacaoEmLoteTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Prateleiras = "api/storage/shelf/v1";
    private const string Alocacoes = "api/storage/shelf/v1/allocation";
    private const string Lote = "api/storage/shelf/v1/allocation/batch";
    private const string Produtos = "api/storage/products/v1";

    private IDialogService _dialogo = null!;

    private static object Produto(int id, decimal? precoInicial, decimal? volume = 1.5m, int deposito = 10) => new
    {
        id,
        name = $"Produto {id}",
        descricao = "Produto de teste",
        qntd = deposito,
        volume,
        precoInicial,
        employeeId = 1,
        dateRegister = "2026-03-01T00:00:00",
    };

    private static object Entrada(int id, int produto, int prateleira, int saldo) => new
    {
        id,
        productId = produto,
        productName = $"Produto {produto}",
        productQuantity = saldo,
        productPrice = 10.0m,
        shelfId = prateleira,
        shelfName = $"Prateleira {prateleira}",
        dateEnter = "2026-09-14T10:30:00",
    };

    private ApiFalsa Monta(object[] produtos, object[] entradas, decimal volumeDaPrimeira = 400m)
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, Prateleiras, new object[]
            {
                new { id = 1, name = "Prateleira A1", volume = volumeDaPrimeira, usedVolume = 0m },
                new { id = 2, name = "Prateleira A2", volume = 400m, usedVolume = 0m },
            })
            .Responde(HttpMethod.Get, Alocacoes, entradas);

        foreach (dynamic produto in produtos)
            api.Responde(HttpMethod.Get, $"{Produtos}/{produto.id}", produto);

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
        Services.AddSingleton<IShelfService>(new ShelfService(api.Cliente()));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaLote(string ids)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo($"product/allocation/batch?ids={ids}");

        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<AllocateProductsBatch>(1);
            builder.CloseComponent();
        });

        cut.WaitForAssertion(() => Assert.Contains("produto(s)", cut.Markup));

        return cut;
    }

    private static AngleSharp.Dom.IElement Botao(IRenderedComponent<IComponent> cut, string texto)
        => cut.FindAll("button").First(b => b.TextContent.Contains(texto));

    [Fact]
    public void Lote_envia_um_post_com_quantidade_1_preco_inicial_e_a_prateleira_atual_de_cada_produto()
    {
        var api = Monta(
            new[] { Produto(41, 25.5m), Produto(42, 7.9m) },
            new[] { Entrada(1, 41, 1, 3), Entrada(2, 42, 2, 5) });
        api.Responde(HttpMethod.Post, Lote, new[] { Entrada(1, 41, 1, 4), Entrada(2, 42, 2, 6) });
        var cut = RenderizaLote("41,42");

        Botao(cut, "Alocar 2 produto(s)").Click();

        cut.WaitForAssertion(() => Assert.Single(api.Requisicoes, r => r.Metodo == HttpMethod.Post));
        var itens = api.JsonDe(HttpMethod.Post, Lote).RootElement.GetProperty("items").EnumerateArray()
            .Select(i => (i.GetProperty("productId").GetInt32(), i.GetProperty("shelfId").GetInt32(),
                          i.GetProperty("qntd").GetInt32(), i.GetProperty("price").GetDecimal()))
            .ToList();

        Assert.Equal(new[] { (41, 1, 1, 25.5m), (42, 2, 1, 7.9m) }, itens);
    }

    [Fact]
    public void Produto_sem_preco_inicial_e_sem_prateleira_bloqueia_o_envio()
    {
        Monta(new[] { Produto(41, 25.5m), Produto(42, null) }, new[] { Entrada(1, 41, 1, 3) });

        var cut = RenderizaLote("41,42");

        Assert.Contains("Produto sem preço inicial", cut.Markup);
        Assert.Equal(string.Empty, cut.FindComponents<MudSelect<int>>()[1].Instance.Text ?? string.Empty);
        Assert.StartsWith("Prateleira A1", cut.FindComponents<MudSelect<int>>()[0].Instance.Text);
        Assert.Contains("Preencha quantidade, preço e prateleira de todos os produtos.", cut.Markup);
        Assert.True(Botao(cut, "Alocar 2 produto(s)").HasAttribute("disabled"));
    }

    [Fact]
    public void Teto_da_prateleira_e_somado_entre_os_itens_do_lote()
    {
        Monta(
            new[] { Produto(41, 10m, volume: 5m), Produto(42, 10m, volume: 5m) },
            new[] { Entrada(1, 41, 1, 1), Entrada(2, 42, 1, 1) },
            volumeDaPrimeira: 10m);

        var cut = RenderizaLote("41,42");

        Assert.Contains("A Prateleira A1 não comporta o lote: são necessários 10 L e restam 9 L.", cut.Markup);
        Assert.False(Botao(cut, "Alocar 2 produto(s)").HasAttribute("disabled"));
    }

    [Fact]
    public void Escolher_outra_prateleira_para_produto_ja_alocado_mostra_a_regra()
    {
        Monta(new[] { Produto(41, 10m) }, new[] { Entrada(1, 41, 1, 3) });
        var cut = RenderizaLote("41");
        Assert.DoesNotContain("já está na", cut.Markup);

        var select = cut.FindComponent<MudSelect<int>>();
        cut.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(2));

        cut.WaitForAssertion(() => Assert.Contains("O produto já está na Prateleira A1.", cut.Markup));
    }

    [Fact]
    public void Tirar_do_lote_remove_o_item_do_envio()
    {
        var api = Monta(
            new[] { Produto(41, 25.5m), Produto(42, 7.9m) },
            new[] { Entrada(1, 41, 1, 3), Entrada(2, 42, 2, 5) });
        api.Responde(HttpMethod.Post, Lote, new[] { Entrada(2, 42, 2, 6) });
        var cut = RenderizaLote("41,42");

        cut.Find("button[aria-label='Tirar Produto 41 do lote']").Click();
        Botao(cut, "Alocar 1 produto(s)").Click();

        cut.WaitForAssertion(() => Assert.Single(api.Requisicoes, r => r.Metodo == HttpMethod.Post));
        var itens = api.JsonDe(HttpMethod.Post, Lote).RootElement.GetProperty("items");
        Assert.Equal(1, itens.GetArrayLength());
        Assert.Equal(42, itens[0].GetProperty("productId").GetInt32());
    }

    [Fact]
    public void Recusa_da_api_mostra_o_item_e_recarrega_a_ocupacao()
    {
        var api = Monta(new[] { Produto(41, 25.5m) }, new[] { Entrada(1, 41, 1, 3) });
        api.Responde(HttpMethod.Post, Lote, "Item 1 (Produto 41 na Prateleira A1): saldo insuficiente no depósito: há 10 e o lote pede 11.", HttpStatusCode.BadRequest);
        var cut = RenderizaLote("41");

        Botao(cut, "Alocar 1 produto(s)").Click();

        cut.WaitForAssertion(() => Assert.Equal(2, api.Requisicoes.Count(r => r.Metodo == HttpMethod.Get && r.Caminho == Prateleiras)));
        var chamada = _dialogo.ReceivedCalls().Last(c => c.GetMethodInfo().Name == "ShowAsync");
        var parametros = (DialogParameters)chamada.GetArguments()[1]!;
        Assert.Contains("Nenhum produto foi alocado.", (string)parametros["ContentText"]!);
        Assert.Contains("Item 1 (Produto 41 na Prateleira A1)", (string)parametros["ContentText"]!);
    }

    [Fact]
    public void Sem_ids_mostra_o_aviso_de_nenhum_produto_selecionado()
    {
        Monta(Array.Empty<object>(), Array.Empty<object>());

        Services.GetRequiredService<NavigationManager>().NavigateTo("product/allocation/batch");
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<AllocateProductsBatch>(1);
            builder.CloseComponent();
        });

        cut.WaitForAssertion(() => Assert.Contains("Nenhum produto selecionado.", cut.Markup));
    }

    [Fact]
    public void Cards_marcados_levam_a_tela_de_lote_com_os_ids()
    {
        var api = Monta(Array.Empty<object>(), Array.Empty<object>());
        api.Responde(HttpMethod.Get, Produtos, new[] { Produto(41, 25.5m), Produto(42, null), Produto(43, 3m) }, total: 3);
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<Products>(1);
            builder.CloseComponent();
        });
        cut.WaitForAssertion(() => Assert.Equal(3, cut.FindAll("input[type=checkbox]").Count));
        Assert.DoesNotContain("Alocar selecionados", cut.Markup);

        cut.FindAll("input[type=checkbox]")[0].Change(true);
        cut.FindAll("input[type=checkbox]")[2].Change(true);

        Botao(cut, "Alocar selecionados (2)").Click();

        Assert.EndsWith("product/allocation/batch?ids=41,43", Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void Alocacao_individual_usa_o_preco_inicial_como_padrao()
    {
        var api = Monta(new[] { Produto(41, 25.5m) }, Array.Empty<object>());
        api.Responde(HttpMethod.Post, Alocacoes, Entrada(1, 41, 1, 1));
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<AllocateProductToShelf>(1);
            builder.AddAttribute(2, nameof(AllocateProductToShelf.productId), 41);
            builder.CloseComponent();
        });
        cut.WaitForElement("form");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Single(api.Requisicoes, r => r.Metodo == HttpMethod.Post));
        Assert.Equal(25.5m, api.JsonDe(HttpMethod.Post, Alocacoes).RootElement.GetProperty("productPrice").GetDecimal());
    }
}
