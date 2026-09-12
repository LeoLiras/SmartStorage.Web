using System.Net.Http;
using System.Text.Json;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Pages.Sale;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// Regressao do defeito de 2026-09-12: vender pela prateleira enviava
/// ProductId zero, porque o estado global entregava um VO vazio em vez de nulo
/// e a tela de prateleiras nunca preenchia o produto. O roteiro de API nao pega
/// isso, ja que no nivel da API o payload chegava "bem formado", so com o campo
/// errado.
/// </summary>
public class RegistroDeVendaTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const int Entrada = 7;
    private const int Produto = 42;
    private const int Prateleira = 3;

    private ApiFalsa Monta(VariablesExtensions app)
    {
        var api = new ApiFalsa();

        api.Responde(HttpMethod.Get, $"api/storage/shelf/v1/allocation/{Entrada}", new
        {
            id = Entrada,
            productId = Produto,
            productName = "Capacete de Seguranca Branco",
            productQuantity = 30,
            productPrice = 10.0m,
            shelfId = Prateleira,
            shelfName = "Prateleira B1",
            dateEnter = "2026-03-10T00:00:00",
        });

        api.Responde(HttpMethod.Get, $"api/storage/products/v1/{Produto}", new
        {
            id = Produto,
            name = "Capacete de Seguranca Branco",
            descricao = "Capacete de protecao classe B",
            qntd = 35,
            employeeId = 1,
            dateRegister = "2026-03-01T00:00:00",
        });

        api.Responde(HttpMethod.Post, "api/storage/sales/v1", new
        {
            id = 1,
            idEnter = Entrada,
            productId = Produto,
            qntd = 3,
            dateSale = "2026-09-12T13:30:00",
        });

        api.Responde(HttpMethod.Get, "api/storage/shelf/v1/allocation", Array.Empty<object>());
        api.Responde(HttpMethod.Get, "api/storage/sales/v1", Array.Empty<object>());

        var dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
               .ReturnsForAnyArgs(Task.FromResult(referencia));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(app);
        Services.AddSingleton(new Dialogo(dialogo));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api)
        {
            BaseAddress = new Uri("http://localhost/"),
        }));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    /// <summary>
    /// O MudDatePicker da tela exige um MudPopoverProvider na arvore, e ele nao
    /// aceita ser wrapper por nao ter ChildContent: por isso os dois entram como
    /// irmaos no mesmo fragmento.
    /// </summary>
    private void Vende(ApiFalsa api, int quantidade)
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<NewSale>(1);
            builder.AddAttribute(2, nameof(NewSale.entryId), Entrada);
            builder.CloseComponent();
        });

        cut.FindAll("input:not([disabled])").First().Change(quantidade.ToString());

        cut.Find("form").Submit();
    }

    [Fact]
    public void Venda_envia_o_produto_da_entrada_mesmo_com_estado_global_vazio()
    {
        var app = new VariablesExtensions();
        var api = Monta(app);

        Vende(api, 3);

        var corpo = api.JsonDe(HttpMethod.Post, "api/storage/sales/v1").RootElement;

        Assert.Equal(Produto, corpo.GetProperty("productId").GetInt32());
        Assert.Equal(Entrada, corpo.GetProperty("idEnter").GetInt32());
        Assert.Equal(3, corpo.GetProperty("qntd").GetInt32());
    }

    [Fact]
    public void Venda_busca_a_entrada_e_o_produto_por_id_quando_o_estado_global_esta_vazio()
    {
        var app = new VariablesExtensions();
        var api = Monta(app);

        Vende(api, 2);

        Assert.Contains(api.Requisicoes,
            r => r.Metodo == HttpMethod.Get &&
                 r.Caminho == $"api/storage/shelf/v1/allocation/{Entrada}");

        Assert.Contains(api.Requisicoes,
            r => r.Metodo == HttpMethod.Get &&
                 r.Caminho == $"api/storage/products/v1/{Produto}");
    }

    [Fact]
    public void Venda_usa_o_estado_global_quando_ele_esta_preenchido_e_nao_busca_por_id()
    {
        var app = new VariablesExtensions
        {
            ActualEntry = new SmartStorage_Shared.VO.EnterVO
            {
                Id = Entrada, ProductId = Produto, ProductQuantity = 30,
                ProductPrice = 10.0m, ShelfId = Prateleira,
                DateEnter = new DateTime(2026, 3, 10),
            },
            ActualProduct = new SmartStorage_Shared.VO.ProductVO
            {
                Id = Produto, Name = "Capacete de Seguranca Branco", Qntd = 35,
            },
        };
        var api = Monta(app);

        Vende(api, 4);

        Assert.DoesNotContain(api.Requisicoes,
            r => r.Metodo == HttpMethod.Get &&
                 r.Caminho == $"api/storage/products/v1/{Produto}");

        var corpo = api.JsonDe(HttpMethod.Post, "api/storage/sales/v1").RootElement;

        Assert.Equal(Produto, corpo.GetProperty("productId").GetInt32());
    }

    /// <summary>
    /// O instante do lancamento no ledger e do servidor, nao desta data: quem
    /// garante isso e o ApplyMovementToBalance, coberto por tests/ledger_e2e.py.
    /// Aqui basta que o SalDateSale saia preenchido, porque ele e o dado de
    /// negocio que o usuario escolhe na tela.
    /// </summary>
    [Fact]
    public void Venda_envia_a_data_escolhida_e_nao_o_valor_default()
    {
        var app = new VariablesExtensions();
        var api = Monta(app);

        Vende(api, 1);

        var corpo = api.JsonDe(HttpMethod.Post, "api/storage/sales/v1").RootElement;
        var data = corpo.GetProperty("dateSale").GetDateTime();

        Assert.NotEqual(default, data);
        Assert.Equal(DateTime.Today, data.Date);
    }
}
