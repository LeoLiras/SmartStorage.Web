using System.Net.Http;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Sale;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class ListagemDeVendasTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Vendas = "api/storage/sales/v1";

    private static object[] Pagina(int quantidade, int primeiroId = 1) => Enumerable.Range(primeiroId, quantidade)
        .Select(id => (object)new
        {
            id,
            idEnter = 3,
            productId = 42,
            productName = $"Produto {id}",
            shelfName = "Prateleira B1",
            salePrice = 10.0m,
            qntd = 1,
            returnedQntd = 0,
            saleTotal = 10.0m,
            dateSale = "2026-09-14T10:30:00",
        })
        .ToArray();

    private ApiFalsa Monta(object[] vendas, int total)
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, Vendas, vendas, total: total);

        var dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
               .ReturnsForAnyArgs(Task.FromResult(referencia));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(dialogo, new SessionExpiration()));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api)
        {
            BaseAddress = new Uri("http://localhost/"),
        }));
        Services.AddSingleton<IShelfService>(new ShelfService(api.Cliente()));
        Services.AddSingleton<ISaleService>(new SaleService(api.Cliente()));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaVendas()
        => Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<Sales>(1);
            builder.CloseComponent();
        });

    [Fact]
    public void Primeira_pagina_pede_dez_vendas_a_api_e_usa_o_total_do_servidor()
    {
        var api = Monta(Pagina(10), total: 25);

        var cut = RenderizaVendas();

        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));
        Assert.Equal(new[] { "?page=1&pageSize=10" }, api.Consultas);
        Assert.Contains("25", cut.Find(".mud-table-pagination-caption").TextContent);
    }

    [Fact]
    public void Proxima_pagina_pede_a_pagina_dois_a_api()
    {
        var api = Monta(Pagina(10), total: 25);
        var cut = RenderizaVendas();
        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));

        cut.Find("button[aria-label='Next page']").Click();

        cut.WaitForAssertion(() => Assert.Contains("?page=2&pageSize=10", api.Consultas));
    }

    [Fact]
    public void Pesquisa_vai_para_a_api_a_partir_da_primeira_pagina()
    {
        var api = Monta(Pagina(10), total: 25);
        var cut = RenderizaVendas();
        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));
        cut.Find("button[aria-label='Next page']").Click();
        cut.WaitForAssertion(() => Assert.Contains("?page=2&pageSize=10", api.Consultas));

        cut.Find("input").Input("capacete branco");

        cut.WaitForAssertion(() => Assert.Equal("?page=1&pageSize=10&search=capacete%20branco", api.Consultas.Last()));
    }

    [Fact]
    public void Sem_vendas_mostra_o_aviso()
    {
        Monta(Pagina(0), total: 0);

        var cut = RenderizaVendas();

        cut.WaitForAssertion(() => Assert.Contains("Não há produtos vendidos.", cut.Markup));
    }
}
