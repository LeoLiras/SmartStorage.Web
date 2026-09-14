using System.Net.Http;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
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

    private string RenderizaListagem(VariablesExtensions app)
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, "api/storage/products/v1", new[]
        {
            new
            {
                id = 42,
                name = "Capacete de Seguranca Branco",
                descricao = "Capacete de protecao classe B",
                qntd = 6,
                shelvesQntd = 4,
                employeeId = 1,
                dateRegister = "2026-03-01T00:00:00",
            },
        });

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(app);
        Services.AddSingleton(new Dialogo(Substitute.For<IDialogService>(), new SessionExpiration()));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api)
        {
            BaseAddress = new Uri("http://localhost/"),
        }));
        AddAuthorization().SetAuthorized("admin");

        return Render<Products>().Markup;
    }

    [Fact]
    public void Card_mostra_o_total_e_a_divisao_entre_deposito_e_prateleiras()
    {
        var markup = RenderizaListagem(new VariablesExtensions());

        Assert.Contains("Estoque total: 10", markup);
        Assert.Contains("Depósito: 6 · Prateleiras: 4", markup);
    }

    [Fact]
    public void Card_nao_oferece_exclusao_de_produto()
    {
        var markup = RenderizaListagem(new VariablesExtensions());

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

        var markup = RenderizaListagem(app);

        Assert.Contains("Estoque total: 10", markup);
        Assert.DoesNotContain("Estoque total: 35", markup);
    }
}
