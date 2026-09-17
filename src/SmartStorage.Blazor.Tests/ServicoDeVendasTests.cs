using System.Net;
using System.Net.Http;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Utils.API;
using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// SaleService da issue #22: as telas de venda deixaram o ApiExtensions, e as
/// rotas, a paginacao e o tratamento de erro passaram a morar aqui.
/// </summary>
public class ServicoDeVendasTests
{
    private const string Vendas = "api/storage/sales/v1";

    private static object Venda(int id) => new { id, idEnter = 7, qntd = 2, dateSale = "2026-09-16T10:00:00" };

    [Fact]
    public async Task Pagina_manda_pagina_tamanho_e_pesquisa_e_le_o_total_do_cabecalho()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, Vendas, new[] { Venda(1), Venda(2) }, total: 12);
        var servico = new SaleService(api.Cliente());

        var (itens, total) = await servico.GetSalesPage(2, 5, " capacete branco ");

        Assert.Equal(new[] { 1, 2 }, itens.Select(v => v.Id));
        Assert.Equal(12, total);
        Assert.Equal("?page=2&pageSize=5&search=capacete%20branco", api.Consultas.Single());
    }

    [Fact]
    public async Task Cancelar_usa_delete_na_venda()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Delete, $"{Vendas}/3", Venda(3));
        var servico = new SaleService(api.Cliente());

        var venda = await servico.CancelSale(3);

        Assert.Equal(3, venda.Id);
        Assert.Single(api.Requisicoes, r => r.Metodo == HttpMethod.Delete && r.Caminho == $"{Vendas}/3");
    }

    [Fact]
    public async Task Devolver_e_carrinho_vao_para_as_rotas_proprias()
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Post, $"{Vendas}/3/return", Venda(3))
            .Responde(HttpMethod.Post, $"{Vendas}/batch", new[] { Venda(4), Venda(5) });
        var servico = new SaleService(api.Cliente());

        await servico.ReturnSale(3, new SaleReturnVO { Quantity = 2 });
        var lote = await servico.CreateSales(new SaleBatchVO
        {
            DateSale = new DateTime(2026, 9, 16),
            Items = { new SaleBatchItemVO { IdEnter = 7, Qntd = 1 } },
        });

        Assert.Equal(2, api.JsonDe(HttpMethod.Post, $"{Vendas}/3/return").RootElement.GetProperty("quantity").GetInt32());
        Assert.Equal(2, lote.Count);
    }

    [Fact]
    public async Task Resposta_de_erro_vira_api_exception_com_a_mensagem_da_api()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Post, Vendas, "A quantidade da venda deve ser maior que zero.", HttpStatusCode.BadRequest);
        var servico = new SaleService(api.Cliente());

        var erro = await Assert.ThrowsAsync<ApiException>(() => servico.CreateSale(new SaleVO { IdEnter = 7 }));

        Assert.Equal(400, erro.StatusCode);
        Assert.Equal("A quantidade da venda deve ser maior que zero.", erro.Message);
    }
}
