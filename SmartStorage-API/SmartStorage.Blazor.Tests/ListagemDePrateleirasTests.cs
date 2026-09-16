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
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Cart;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class ListagemDePrateleirasTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Alocacoes = "api/storage/shelf/v1/allocation";

    private static object Entrada(int id) => new
    {
        id,
        productId = 42,
        productName = $"Produto {id}",
        productQuantity = 5,
        productPrice = 10.0m,
        shelfId = 1,
        shelfName = "Prateleira A1",
        dateEnter = "2026-09-14T10:30:00",
    };

    private static object[] Pagina(int quantidade, int primeiroId = 1)
        => Enumerable.Range(primeiroId, quantidade).Select(Entrada).ToArray();

    private ApiFalsa Monta(object[] entradas, int total)
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, Alocacoes, entradas, total: total)
            .Responde(HttpMethod.Put, $"{Alocacoes}/1", Entrada(1));

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
        Services.AddScoped<SaleCart>();
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaPrateleiras()
        => Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<ProductsShelves>(1);
            builder.CloseComponent();
        });

    private static List<string> ConsultasDaListagem(ApiFalsa api)
        => api.Requisicoes.Select((r, i) => (r, i))
              .Where(x => x.r.Metodo == HttpMethod.Get && x.r.Caminho == Alocacoes)
              .Select(x => api.Consultas[x.i])
              .ToList();

    [Fact]
    public void Primeira_pagina_pede_dez_alocacoes_a_api_e_usa_o_total_do_servidor()
    {
        var api = Monta(Pagina(10), total: 25);

        var cut = RenderizaPrateleiras();

        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));
        Assert.Equal(new[] { "?page=1&pageSize=10" }, ConsultasDaListagem(api));
        Assert.Contains("25", cut.Find(".mud-table-pagination-caption").TextContent);
    }

    [Fact]
    public void Proxima_pagina_pede_a_pagina_dois_a_api()
    {
        var api = Monta(Pagina(10), total: 25);
        var cut = RenderizaPrateleiras();
        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));

        cut.Find("button[aria-label='Next page']").Click();

        cut.WaitForAssertion(() => Assert.Contains("?page=2&pageSize=10", ConsultasDaListagem(api)));
    }

    [Fact]
    public void Pesquisa_vai_para_a_api_a_partir_da_primeira_pagina()
    {
        var api = Monta(Pagina(10), total: 25);
        var cut = RenderizaPrateleiras();
        cut.WaitForAssertion(() => Assert.Contains("Produto 10", cut.Markup));
        cut.Find("button[aria-label='Next page']").Click();
        cut.WaitForAssertion(() => Assert.Contains("?page=2&pageSize=10", ConsultasDaListagem(api)));

        cut.Find("input").Input("capacete branco");

        cut.WaitForAssertion(() => Assert.Equal("?page=1&pageSize=10&search=capacete%20branco", ConsultasDaListagem(api).Last()));
    }

    [Fact]
    public void Sem_alocacoes_mostra_o_aviso()
    {
        Monta(Pagina(0), total: 0);

        var cut = RenderizaPrateleiras();

        cut.WaitForAssertion(() => Assert.Contains("Não há produtos alocados para venda nas prateleiras.", cut.Markup));
    }

    [Fact]
    public void Desfazer_alocacao_manda_so_o_put_e_recarrega_a_pagina_pela_api()
    {
        var api = Monta(Pagina(3), total: 3);
        var cut = RenderizaPrateleiras();
        cut.WaitForAssertion(() => Assert.Contains("Produto 3", cut.Markup));

        cut.Find("button[aria-label='Enviar de volta para o estoque']").Click();

        cut.WaitForAssertion(() => Assert.Equal(2, ConsultasDaListagem(api).Count));
        Assert.Single(api.Requisicoes, r => r.Metodo == HttpMethod.Put && r.Caminho == $"{Alocacoes}/1" && r.Corpo == "");
        Assert.DoesNotContain(api.Requisicoes, r => r.Metodo == HttpMethod.Get && r.Caminho == $"{Alocacoes}/1");
    }
}
