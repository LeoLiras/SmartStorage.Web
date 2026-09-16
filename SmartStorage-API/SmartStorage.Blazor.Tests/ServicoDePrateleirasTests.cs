using System.Net;
using System.Net.Http;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Utils.API;
using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// ShelfService da issue #22: prateleiras e alocacoes deixaram o ApiExtensions,
/// e as rotas de lote, transferencia e desfazer passaram a morar aqui.
/// </summary>
public class ServicoDePrateleirasTests
{
    private const string Prateleiras = "api/storage/shelf/v1";

    private const string Alocacoes = "api/storage/shelf/v1/allocation";

    private static object Entrada(int id) => new { id, productId = 42, productName = "Capacete", productQuantity = 5, shelfId = 1, shelfName = "A1" };

    [Fact]
    public async Task Prateleiras_e_pagina_de_alocacoes_vao_para_as_rotas_certas()
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, Prateleiras, new[] { new { id = 1, name = "A1" } })
            .Responde(HttpMethod.Get, Alocacoes, new[] { Entrada(1), Entrada(2) }, total: 9);
        var servico = new ShelfService(api.Cliente());

        var prateleiras = await servico.GetShelves();
        var (itens, total) = await servico.GetAllocationsPage(1, 10, " capacete ");

        Assert.Equal("A1", prateleiras.Single().Name);
        Assert.Equal(new[] { 1, 2 }, itens.Select(e => e.Id));
        Assert.Equal(9, total);
        Assert.Equal("?page=1&pageSize=10&search=capacete", api.Consultas.Last());
    }

    [Fact]
    public async Task Desfazer_usa_put_sem_corpo_na_alocacao()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Put, $"{Alocacoes}/3", Entrada(3));
        var servico = new ShelfService(api.Cliente());

        var entrada = await servico.UndoAllocation(3);

        Assert.Equal(3, entrada.Id);
        Assert.Single(api.Requisicoes, r => r.Metodo == HttpMethod.Put && r.Caminho == $"{Alocacoes}/3" && r.Corpo == "");
    }

    [Fact]
    public async Task Lote_e_transferencia_vao_para_as_rotas_proprias()
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Post, $"{Alocacoes}/batch", new[] { Entrada(4), Entrada(5) })
            .Responde(HttpMethod.Post, $"{Alocacoes}/3/transfer", Entrada(6));
        var servico = new ShelfService(api.Cliente());

        var lote = await servico.AllocateProducts(new AllocationBatchVO
        {
            Items = { new AllocationBatchItemVO { ProductId = 42, ShelfId = 1, Qntd = 2, Price = 15.90m } },
        });
        var transferida = await servico.TransferAllocation(3, new ShelfTransferVO { ShelfId = 2 });

        Assert.Equal(2, lote.Count);
        Assert.Equal(6, transferida.Id);
        Assert.Equal(2, api.JsonDe(HttpMethod.Post, $"{Alocacoes}/3/transfer").RootElement.GetProperty("shelfId").GetInt32());
    }

    [Fact]
    public async Task Resposta_de_erro_vira_api_exception_com_a_mensagem_da_api()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Post, Alocacoes, "Quantidade maior que o saldo do depósito.", HttpStatusCode.BadRequest);
        var servico = new ShelfService(api.Cliente());

        var erro = await Assert.ThrowsAsync<ApiException>(() => servico.AllocateProduct(new EnterVO { ProductId = 42 }));

        Assert.Equal(400, erro.StatusCode);
        Assert.Equal("Quantidade maior que o saldo do depósito.", erro.Message);
    }
}
