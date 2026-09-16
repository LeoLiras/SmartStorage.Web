using System.Net.Http;
using AngleSharp.Dom;
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
using SmartStorage_Shared.VO;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class DevolucaoDeVendaTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const int Venda = 7;

    private static object VendaComDevolucao(int vendida, int devolvida) => new
    {
        id = Venda,
        idEnter = 3,
        productId = 42,
        productName = "Capacete de Seguranca Branco",
        shelfName = "Prateleira B1",
        salePrice = 10.0m,
        qntd = vendida,
        returnedQntd = devolvida,
        saleTotal = 10.0m * (vendida - devolvida),
        dateSale = "2026-09-14T10:30:00",
    };

    private ApiFalsa Monta(object venda, VariablesExtensions? app = null)
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, $"api/storage/sales/v1/{Venda}", venda)
            .Responde(HttpMethod.Post, $"api/storage/sales/v1/{Venda}/return", venda)
            .Responde(HttpMethod.Get, "api/storage/sales/v1", new[] { venda })
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

        var dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
               .ReturnsForAnyArgs(Task.FromResult(referencia));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(app ?? new VariablesExtensions());
        Services.AddSingleton(new Dialogo(dialogo, new SessionExpiration()));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api)
        {
            BaseAddress = new Uri("http://localhost/"),
        }));
        Services.AddSingleton<ISaleService>(new SaleService(api.Cliente()));
        Services.AddSingleton<IShelfService>(new ShelfService(api.Cliente()));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaDevolucao(bool esperaForm = true)
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<SaleReturn>(1);
            builder.AddAttribute(2, nameof(SaleReturn.saleId), Venda);
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
    public void Devolver_envia_a_quantidade_para_a_rota_de_devolucao_da_venda()
    {
        var api = Monta(VendaComDevolucao(vendida: 5, devolvida: 1));
        var cut = RenderizaDevolucao();

        Campo(cut, "Quantidade a devolver").Change("2");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Contains(api.Requisicoes, r => r.Metodo == HttpMethod.Post));
        var escritas = api.Requisicoes.Where(r => r.Metodo != HttpMethod.Get).ToList();
        Assert.Single(escritas);
        Assert.Equal($"api/storage/sales/v1/{Venda}/return", escritas[0].Caminho);

        var corpo = api.JsonDe(HttpMethod.Post, $"api/storage/sales/v1/{Venda}/return").RootElement;
        Assert.Equal(2, corpo.GetProperty("quantity").GetInt32());
    }

    [Fact]
    public void Tela_de_devolucao_usa_o_form_de_produto_so_para_leitura()
    {
        Monta(VendaComDevolucao(vendida: 5, devolvida: 1));
        var cut = RenderizaDevolucao();

        Assert.Equal("Capacete de Seguranca Branco", Campo(cut, "Nome do Produto").GetAttribute("value"));
        Assert.True(Campo(cut, "Nome do Produto").HasAttribute("readonly"));
        Assert.DoesNotContain("Ajustar estoque do depósito", cut.Markup);
        Assert.DoesNotContain("Adicionar Imagem", cut.Markup);

        Assert.Equal("5", Campo(cut, "Quantidade vendida").GetAttribute("value"));
        Assert.Equal("1", Campo(cut, "Já devolvida").GetAttribute("value"));
        Assert.False(Campo(cut, "Quantidade a devolver").HasAttribute("readonly"));
        Assert.Contains("Até 4", cut.Markup);
        Assert.Equal("Devolver", cut.Find("button[type=submit]").TextContent.Trim());
    }

    [Fact]
    public void Form_ignora_produto_do_estado_global_quando_o_id_e_outro()
    {
        var app = new VariablesExtensions
        {
            ActualProduct = new ProductVO { Id = 99, Name = "Produto de outra tela", Descricao = "Nao e este" },
        };
        Monta(VendaComDevolucao(vendida: 5, devolvida: 1), app);
        var cut = RenderizaDevolucao();

        Assert.Equal("Capacete de Seguranca Branco", Campo(cut, "Nome do Produto").GetAttribute("value"));
    }

    [Fact]
    public void Venda_totalmente_devolvida_nao_oferece_nova_devolucao()
    {
        var api = Monta(VendaComDevolucao(vendida: 5, devolvida: 5));
        var cut = RenderizaDevolucao(esperaForm: false);

        Assert.Contains("já foi totalmente devolvida", cut.Markup);
        Assert.Empty(cut.FindAll("button[type=submit]"));
        Assert.DoesNotContain("Nome do Produto", cut.Markup);
        Assert.DoesNotContain(api.Requisicoes, r => r.Metodo != HttpMethod.Get);
    }

    [Fact]
    public void Listagem_mostra_quantidade_e_total_liquidos_e_leva_a_devolucao()
    {
        var app = new VariablesExtensions
        {
            Sales = new List<SaleVO> { new() { Id = Venda, ProductName = "Venda antiga em cache", Qntd = 5 } },
        };
        Monta(VendaComDevolucao(vendida: 5, devolvida: 2), app);

        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<Sales>(1);
            builder.CloseComponent();
        });

        var celulas = cut.FindAll("td").Select(td => td.TextContent.Trim()).ToList();
        Assert.Contains("Capacete de Seguranca Branco", celulas);
        Assert.Contains("3", celulas);
        Assert.Contains("30,00", celulas);
        Assert.DoesNotContain("Venda antiga em cache", cut.Markup);

        cut.FindAll("td button").First().Click();

        Assert.EndsWith($"product/sale/return/{Venda}", Services.GetRequiredService<NavigationManager>().Uri);
    }
}
