using System.Net.Http;
using System.Text.RegularExpressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using SmartStorage_Shared.VO;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class ListagemDeProdutosTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Produtos = "api/storage/products/v1";

    private static readonly object Capacete = new
    {
        id = 42,
        name = "Capacete de Seguranca Branco",
        descricao = "Capacete de protecao classe B",
        qntd = 6,
        shelvesQntd = 4,
        employeeId = 1,
        dateRegister = "2026-03-01T00:00:00",
    };

    private static object[] Pagina(int quantidade, int primeiroId = 1) => Enumerable.Range(primeiroId, quantidade)
        .Select(id => (object)new
        {
            id,
            name = $"Produto {id}",
            descricao = "Produto de teste",
            qntd = 1,
            shelvesQntd = 0,
            employeeId = 1,
            dateRegister = "2026-03-01T00:00:00",
        })
        .ToArray();

    private ApiFalsa Monta(object[] produtos, int total, VariablesExtensions? app = null)
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, Produtos, produtos, total: total);

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(app ?? new VariablesExtensions());
        Services.AddSingleton(new Dialogo(Substitute.For<IDialogService>(), new SessionExpiration()));
        Services.AddSingleton<IProductService>(new ProductService(api.Cliente()));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaProdutos()
        => Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<Products>(1);
            builder.CloseComponent();
        });

    private IRenderedComponent<IComponent> RenderizaCapacete(VariablesExtensions? app = null, object? produto = null)
    {
        Monta(new[] { produto ?? Capacete }, total: 1, app);

        var cut = RenderizaProdutos();
        cut.WaitForAssertion(() => Assert.Contains("Capacete de Seguranca Branco", cut.Markup));

        return cut;
    }

    private static string TextoDoCard(IRenderedComponent<IComponent> cut)
        => Regex.Replace(cut.Find(".app-product-card").TextContent, @"\s+", " ").Trim();

    [Fact]
    public void Card_mostra_o_total_e_a_divisao_entre_deposito_e_prateleiras()
    {
        var texto = TextoDoCard(RenderizaCapacete());

        Assert.Contains("Estoque total 10", texto);
        Assert.Contains("Depósito 6 · Prateleiras 4", texto);
    }

    [Fact]
    public void Card_sem_volume_e_sem_minimo_avisa_so_a_falta_de_volume()
    {
        var texto = TextoDoCard(RenderizaCapacete());

        Assert.Contains("sem volume", texto);
        Assert.DoesNotContain("mínimo", texto);
    }

    [Fact]
    public void Card_abaixo_do_minimo_mostra_o_minimo_e_o_volume()
    {
        var produto = new
        {
            id = 42,
            name = "Capacete de Seguranca Branco",
            qntd = 1,
            shelvesQntd = 4,
            minimumStock = 6,
            volume = 6.5m,
            employeeId = 1,
            dateRegister = "2026-03-01T00:00:00",
        };

        var texto = TextoDoCard(RenderizaCapacete(produto: produto));

        Assert.Contains("abaixo do mínimo 6", texto);
        Assert.Contains("6,5 L", texto);
        Assert.DoesNotContain("sem volume", texto);
    }

    [Fact]
    public void Card_nao_oferece_exclusao_de_produto()
    {
        var markup = RenderizaCapacete().Markup;

        Assert.Contains("Editar", markup);
        Assert.DoesNotContain("Excluír", markup);
    }

    [Fact]
    public void Listagem_busca_os_saldos_na_api_mesmo_com_produtos_ja_carregados()
    {
        var app = new VariablesExtensions
        {
            ProductInStock = new List<ProductVO>
            {
                new() { Id = 42, Name = "Capacete de Seguranca Branco", Qntd = 35 },
            },
        };

        var texto = TextoDoCard(RenderizaCapacete(app));

        Assert.Contains("Estoque total 10", texto);
        Assert.DoesNotContain("Estoque total 35", texto);
    }

    [Fact]
    public void Primeira_pagina_pede_dez_produtos_a_api_e_pagina_pelo_total_do_servidor()
    {
        var api = Monta(Pagina(10), total: 25);

        var cut = RenderizaProdutos();

        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));
        Assert.Equal(new[] { "?page=1&pageSize=10" }, api.Consultas);
        Assert.Equal(3, cut.FindComponent<MudPagination>().Instance.Count);
    }

    [Fact]
    public void Trocar_de_pagina_pede_a_pagina_a_api()
    {
        var api = Monta(Pagina(10), total: 25);
        var cut = RenderizaProdutos();
        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));

        cut.Find("button[aria-label='Next page']").Click();

        cut.WaitForAssertion(() => Assert.Contains("?page=2&pageSize=10", api.Consultas));
    }

    [Fact]
    public void Pesquisa_vai_para_a_api_a_partir_da_primeira_pagina()
    {
        var api = Monta(Pagina(10), total: 25);
        var cut = RenderizaProdutos();
        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));
        cut.Find("button[aria-label='Next page']").Click();
        cut.WaitForAssertion(() => Assert.Contains("?page=2&pageSize=10", api.Consultas));

        cut.Find("input").Input("capacete branco");

        cut.WaitForAssertion(() => Assert.Equal("?page=1&pageSize=10&search=capacete%20branco", api.Consultas.Last()));
        cut.WaitForAssertion(() => Assert.Equal(1, cut.FindComponent<MudPagination>().Instance.Selected));
    }

    [Fact]
    public void Sem_produtos_mostra_o_aviso()
    {
        Monta(Pagina(0), total: 0);

        var cut = RenderizaProdutos();

        cut.WaitForAssertion(() => Assert.Contains("Não há produtos registrados no banco de dados.", cut.Markup));
    }

    [Fact]
    public void Editar_leva_o_produto_do_card()
    {
        var app = new VariablesExtensions();
        var cut = RenderizaCapacete(app);

        cut.FindAll("button").First(b => b.TextContent.Trim() == "Editar").Click();

        Assert.Equal(42, app.ActualProduct?.Id);
        Assert.EndsWith("product/edit/42", Services.GetRequiredService<NavigationManager>().Uri);
    }
}
