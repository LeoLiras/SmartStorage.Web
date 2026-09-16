using System.Net.Http;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Allocation;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using SmartStorage_Shared.VO;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class AlocacaoEmPrateleiraTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const int Produto = 42;

    private void Monta(decimal? volumeDoProduto, VariablesExtensions? app = null)
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, $"api/storage/products/v1/{Produto}", new
            {
                id = Produto,
                name = "Capacete de Seguranca Branco",
                descricao = "Capacete de protecao classe B",
                qntd = 30,
                volume = volumeDoProduto,
                employeeId = 1,
                dateRegister = "2026-03-01T00:00:00",
            })
            .Responde(HttpMethod.Get, "api/storage/shelf/v1", new object[]
            {
                new { id = 1, name = "Prateleira A1", volume = 400m, usedVolume = 180m },
                new { id = 2, name = "Prateleira B1", volume = (decimal?)null, usedVolume = 0m },
            });

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(app ?? new VariablesExtensions());
        Services.AddSingleton(new Dialogo(Substitute.For<IDialogService>(), new SessionExpiration()));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api)
        {
            BaseAddress = new Uri("http://localhost/"),
        }));
        AddAuthorization().SetAuthorized("admin");
    }

    private IRenderedComponent<IComponent> Renderiza()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<AllocateProductToShelf>(1);
            builder.AddAttribute(2, nameof(AllocateProductToShelf.productId), Produto);
            builder.CloseComponent();
        });

        cut.WaitForElement("form");

        return cut;
    }

    [Fact]
    public void Select_mostra_a_ocupacao_sobre_90_por_cento_do_volume()
    {
        Monta(volumeDoProduto: 2.5m);
        var cut = Renderiza();

        var select = cut.FindComponent<MudSelect<int>>();
        cut.InvokeAsync(() => select.Instance.OpenMenu());

        cut.WaitForAssertion(() => Assert.Contains("Prateleira A1 · 50% ocupada · 180 L livres", cut.Markup));
        Assert.Contains("Prateleira B1 · sem volume cadastrado", cut.Markup);
    }

    [Fact]
    public void Campo_de_prateleira_comeca_vazio_e_mostra_a_prateleira_escolhida()
    {
        Monta(volumeDoProduto: 2.5m);
        var cut = Renderiza();
        var select = cut.FindComponent<MudSelect<int>>();

        Assert.Equal(string.Empty, select.Instance.Text ?? string.Empty);

        Escolhe(cut, "Quantidade", "4", "Prateleira A1");

        cut.WaitForAssertion(() => Assert.Equal("Prateleira A1 · 50% ocupada · 180 L livres", select.Instance.Text), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Ocupacao_vem_da_api_mesmo_com_prateleiras_em_cache()
    {
        var app = new VariablesExtensions
        {
            Shelves = new List<ShelfVO> { new() { Id = 1, Name = "Prateleira A1", Volume = 400m, UsedVolume = 0m } },
        };
        Monta(volumeDoProduto: 2.5m, app);
        var cut = Renderiza();

        var select = cut.FindComponent<MudSelect<int>>();
        cut.InvokeAsync(() => select.Instance.OpenMenu());

        cut.WaitForAssertion(() => Assert.Contains("Prateleira A1 · 50% ocupada", cut.Markup));
        Assert.Equal(180m, app.Shelves.Single(s => s.Id == 1).UsedVolume);
    }

    [Fact]
    public void Mostra_o_volume_necessario_para_a_quantidade()
    {
        Monta(volumeDoProduto: 2.5m);
        var cut = Renderiza();

        var quantidade = cut.FindAll("label").First(l => l.TextContent.Trim() == "Quantidade")
            .Closest(".mud-input-control")!.QuerySelector("input")!;
        quantidade.Change("4");

        var necessario = cut.FindAll("label").First(l => l.TextContent.Trim() == "Volume necessário (L)")
            .Closest(".mud-input-control")!.QuerySelector("input")!;
        cut.WaitForAssertion(() => Assert.Equal("10", necessario.GetAttribute("value")));
        Assert.DoesNotContain("não tem volume cadastrado", cut.Markup);
    }

    private static void Escolhe(IRenderedComponent<IComponent> cut, string rotulo, string valor, string prateleira)
    {
        cut.FindAll("label").First(l => l.TextContent.Trim() == rotulo)
            .Closest(".mud-input-control")!.QuerySelector("input")!.Change(valor);

        var select = cut.FindComponent<MudSelect<int>>();
        cut.InvokeAsync(() => select.Instance.OpenMenu());
        cut.WaitForAssertion(() => Assert.Contains(cut.FindAll(".mud-list-item"), i => i.TextContent.Contains(prateleira)), TimeSpan.FromSeconds(5));
        cut.FindAll(".mud-list-item").First(i => i.TextContent.Contains(prateleira)).Click();
    }

    [Fact]
    public void Prateleira_que_nao_comporta_mostra_o_alerta_e_o_teto_util()
    {
        Monta(volumeDoProduto: 2.5m);
        var cut = Renderiza();

        Escolhe(cut, "Quantidade", "100", "Prateleira A1");

        cut.WaitForAssertion(() => Assert.Contains("A Prateleira A1 não comporta a alocação: são necessários 250 L e restam 180 L.", cut.Markup), TimeSpan.FromSeconds(5));
        Assert.Contains("Teto útil: 90% de", cut.Markup);
    }

    [Fact]
    public void Prateleira_que_comporta_nao_mostra_o_alerta()
    {
        Monta(volumeDoProduto: 2.5m);
        var cut = Renderiza();

        Escolhe(cut, "Quantidade", "4", "Prateleira A1");

        cut.WaitForAssertion(() => Assert.Contains("Teto útil: 90% de", cut.Markup), TimeSpan.FromSeconds(5));
        Assert.DoesNotContain("não comporta", cut.Markup);
    }

    [Fact]
    public void Produto_sem_volume_mostra_o_aviso()
    {
        Monta(volumeDoProduto: null);
        var cut = Renderiza();

        Assert.Contains("Este produto não tem volume cadastrado", cut.Markup);
        Assert.DoesNotContain("Volume necessário", cut.Markup);
    }
}
