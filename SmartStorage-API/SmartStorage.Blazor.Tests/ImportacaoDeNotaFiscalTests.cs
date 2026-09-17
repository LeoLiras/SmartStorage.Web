using System.Net;
using System.Net.Http;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class ImportacaoDeNotaFiscalTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string CaminhoPrevia = "api/storage/invoices/v1/preview";
    private const string CaminhoImportacao = "api/storage/invoices/v1";
    private const string Xml = "<nfeProc><NFe>conteudo da nota</NFe></nfeProc>";

    private IDialogService _dialogo = null!;

    private static object Previa(string? importadaEm = null) => new
    {
        chave = "35260912345678000195550010000012341000012345",
        numero = "1234",
        serie = "1",
        emitenteCnpj = "12345678000195",
        emitenteNome = "Distribuidora de Ferramentas Ltda",
        dataEmissao = "2026-09-15T10:30:00",
        importadaEm,
        items = new object[]
        {
            new { numero = 1, codigo = "7891000100103", codigoInvalido = (string?)null, descricao = "Martelo Unha 27mm Tramontina", unidade = "CX", qntdNota = 2m, valorUnitario = 120m, valorTotal = 240m, productId = (int?)4, productName = "Martelo Unha 27mm", fatorConversao = 6, qntdEstoque = (int?)12 },
            new { numero = 2, codigo = (string?)null, codigoInvalido = "7891000100104", descricao = "Trena de Aco 5 Metros", unidade = "UN", qntdNota = 10m, valorUnitario = 15m, valorTotal = 150m, productId = (int?)null, productName = (string?)null, fatorConversao = 1, qntdEstoque = (int?)10 },
        },
    };

    private ApiFalsa Monta(object previa, HttpStatusCode statusImportacao = HttpStatusCode.OK)
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Post, CaminhoPrevia, previa)
            .Responde(HttpMethod.Get, "api/storage/products/v1", new object[]
            {
                new { id = 7, name = "Capacete de Seguranca Branco", descricao = "Capacete classe B", qntd = 5, fatorConversao = 1, employeeId = 1, dateRegister = "2026-03-01T00:00:00" },
                new { id = 4, name = "Martelo Unha 27mm", descricao = "Martelo de aco forjado", qntd = 40, fatorConversao = 6, employeeId = 2, dateRegister = "2026-03-01T00:00:00" },
            })
            .Responde(HttpMethod.Get, "api/storage/employees/v1", new object[]
            {
                new { id = 1, name = "Ana Paula Ribeiro", rg = "MG1234567", cpf = "52998224725" },
                new { id = 6, name = "Colaborador Padrão", rg = "NA0000000", cpf = "12345678909" },
            });

        if (statusImportacao == HttpStatusCode.OK)
            api.Responde(HttpMethod.Post, CaminhoImportacao, new { id = 1, numero = "1234", serie = "1", itemsCount = 2, createdProducts = 1 });
        else
            api.Responde(HttpMethod.Post, CaminhoImportacao, "Item 2 (Trena de Aco 5 Metros): já existe um produto chamado Trena de Aco 5 Metros.", statusImportacao);

        _dialogo = Substitute.For<IDialogService>();

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(_dialogo, new SessionExpiration()));
        Services.AddSingleton<IInvoiceService>(new InvoiceService(api.Cliente()));
        Services.AddSingleton<IProductService>(new ProductService(api.Cliente()));
        Services.AddSingleton<IEmployeeService>(new EmployeeService(api.Cliente()));
        AddAuthorization().SetAuthorized("admin");

        return api;
    }

    private IRenderedComponent<IComponent> RenderizaComNota()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<ImportInvoice>(1);
            builder.CloseComponent();
        });

        cut.WaitForAssertion(() => cut.FindComponent<InputFile>(), TimeSpan.FromSeconds(5));
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText(Xml, "nota.xml"));
        cut.WaitForAssertion(() => Assert.Contains("Distribuidora de Ferramentas Ltda", cut.Markup), TimeSpan.FromSeconds(5));

        return cut;
    }

    private static IRenderedComponent<MudSelect<int?>> Select(IRenderedComponent<IComponent> cut, string rotulo, int indice = 0)
        => cut.FindComponents<MudSelect<int?>>().Where(s => s.Instance.Label == rotulo).ElementAt(indice);

    private static IRenderedComponent<MudNumericField<int>> Numero(IRenderedComponent<IComponent> cut, string rotulo, int indice)
        => cut.FindComponents<MudNumericField<int>>().Where(s => s.Instance.Label == rotulo).ElementAt(indice);

    private static IElement BotaoImportar(IRenderedComponent<IComponent> cut)
        => cut.FindAll("button").First(b => b.TextContent.Trim() == "Importar nota");

    [Fact]
    public void Ler_o_xml_mostra_o_item_existente_e_o_item_novo()
    {
        var api = Monta(Previa());
        var cut = RenderizaComNota();

        Assert.Equal(Xml, api.JsonDe(HttpMethod.Post, CaminhoPrevia).RootElement.GetProperty("xml").GetString());
        Assert.Equal(4, Select(cut, "Produto", 0).Instance.Value);
        Assert.Contains("Encontrado pelo código de barras", cut.Markup);
        Assert.Null(Select(cut, "Produto", 1).Instance.Value);
        Assert.Equal(6, Select(cut, "Colaborador Responsável").Instance.Value);
        Assert.Equal("Trena de Aco 5 Metros", cut.FindComponents<MudTextField<string>>().Single(t => t.Instance.Label == "Nome do Produto").Instance.Value);
        Assert.Contains("O código 7891000100104 da nota não é um GTIN válido", cut.Markup);
        Assert.Contains("1 existente(s) · 1 novo(s)", cut.Markup);
        Assert.Single(cut.FindComponents<ProductImagePicker>());
        Assert.False(BotaoImportar(cut).HasAttribute("disabled"));
    }

    [Fact]
    public void Importar_envia_o_item_existente_e_o_produto_novo()
    {
        var api = Monta(Previa());
        var cut = RenderizaComNota();

        var colaborador = Select(cut, "Colaborador Responsável");
        cut.InvokeAsync(() => colaborador.Instance.ValueChanged.InvokeAsync(1));
        cut.WaitForAssertion(() => Assert.Equal(1, Select(cut, "Colaborador Responsável").Instance.Value), TimeSpan.FromSeconds(5));

        BotaoImportar(cut).Click();

        cut.WaitForAssertion(() => Assert.Contains(api.Requisicoes, r => r.Metodo == HttpMethod.Post && r.Caminho == CaminhoImportacao), TimeSpan.FromSeconds(5));
        var corpo = api.JsonDe(HttpMethod.Post, CaminhoImportacao).RootElement;
        Assert.Equal(Xml, corpo.GetProperty("xml").GetString());
        var itens = corpo.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(2, itens.Count);

        Assert.Equal(1, itens[0].GetProperty("numero").GetInt32());
        Assert.Equal(4, itens[0].GetProperty("productId").GetInt32());
        Assert.Equal(6, itens[0].GetProperty("fatorConversao").GetInt32());
        Assert.Equal(12, itens[0].GetProperty("qntdEstoque").GetInt32());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, itens[0].GetProperty("newProduct").ValueKind);

        Assert.Equal(System.Text.Json.JsonValueKind.Null, itens[1].GetProperty("productId").ValueKind);
        Assert.Equal(10, itens[1].GetProperty("qntdEstoque").GetInt32());
        var novo = itens[1].GetProperty("newProduct");
        Assert.Equal("Trena de Aco 5 Metros", novo.GetProperty("name").GetString());
        Assert.Equal("Trena de Aco 5 Metros", novo.GetProperty("descricao").GetString());
        Assert.Equal(1, novo.GetProperty("employeeId").GetInt32());
    }

    [Fact]
    public void Fator_recalcula_a_quantidade_ate_ela_ser_editada()
    {
        Monta(Previa());
        var cut = RenderizaComNota();

        cut.InvokeAsync(() => Numero(cut, "Fator de conversão", 1).Instance.ValueChanged.InvokeAsync(12));
        cut.WaitForAssertion(() => Assert.Equal(120, Numero(cut, "Quantidade de entrada", 1).Instance.Value), TimeSpan.FromSeconds(5));

        cut.InvokeAsync(() => Numero(cut, "Quantidade de entrada", 1).Instance.ValueChanged.InvokeAsync(100));
        cut.InvokeAsync(() => Numero(cut, "Fator de conversão", 1).Instance.ValueChanged.InvokeAsync(6));

        cut.WaitForAssertion(() => Assert.Equal(6, Numero(cut, "Fator de conversão", 1).Instance.Value), TimeSpan.FromSeconds(5));
        Assert.Equal(100, Numero(cut, "Quantidade de entrada", 1).Instance.Value);
    }

    [Fact]
    public void Ligar_o_item_a_um_produto_existente_usa_o_fator_dele_e_esconde_o_cadastro()
    {
        Monta(Previa());
        var cut = RenderizaComNota();

        cut.InvokeAsync(() => Select(cut, "Produto", 1).Instance.ValueChanged.InvokeAsync(4));

        cut.WaitForAssertion(() => Assert.Empty(cut.FindComponents<ProductImagePicker>()), TimeSpan.FromSeconds(5));
        Assert.Equal(6, Numero(cut, "Fator de conversão", 1).Instance.Value);
        Assert.Equal(60, Numero(cut, "Quantidade de entrada", 1).Instance.Value);
        Assert.Contains("2 existente(s) · 0 novo(s)", cut.Markup);
    }

    [Fact]
    public void Nota_ja_importada_nao_pode_ser_enviada()
    {
        var api = Monta(Previa(importadaEm: "2026-09-16T08:00:00"));
        var cut = RenderizaComNota();

        Assert.Contains("Esta nota já foi importada em 16/09/2026 08:00.", cut.Markup);
        Assert.True(BotaoImportar(cut).HasAttribute("disabled"));
        Assert.DoesNotContain(api.Requisicoes, r => r.Caminho == CaminhoImportacao);
    }

    [Fact]
    public void Recusa_da_api_avisa_e_mantem_a_conferencia()
    {
        Monta(Previa(), HttpStatusCode.BadRequest);
        var cut = RenderizaComNota();

        BotaoImportar(cut).Click();

        cut.WaitForAssertion(() => _dialogo.ReceivedWithAnyArgs().ShowAsync<Pages.Dialog.Dialog>(default, default, default), TimeSpan.FromSeconds(5));
        Assert.Contains("Distribuidora de Ferramentas Ltda", cut.Markup);
        Assert.Equal("Trena de Aco 5 Metros", cut.FindComponents<MudTextField<string>>().Single(t => t.Instance.Label == "Nome do Produto").Instance.Value);
    }

    [Fact]
    public void Listagem_de_produtos_leva_a_importacao()
    {
        var api = Monta(Previa());
        api.Responde(HttpMethod.Get, "api/storage/products/v1", Array.Empty<object>(), total: 0);

        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<Products>(1);
            builder.CloseComponent();
        });

        var botao = cut.WaitForElement("a[href='product/invoice/import']");
        Assert.Contains("Importar NF-e", botao.TextContent);
    }
}
