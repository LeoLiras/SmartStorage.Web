using System.Net;
using System.Net.Http;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Utils.API;
using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// ProductService da issue #22: as telas de produto deixaram o ApiExtensions,
/// e as rotas, a paginacao e o tratamento de erro passaram a morar aqui.
/// </summary>
public class ServicoDeProdutosTests
{
    private const string Produtos = "api/storage/products/v1";

    private static object Produto(int id) => new { id, name = $"Produto {id}", qntd = 4 };

    [Fact]
    public async Task Pagina_manda_pagina_tamanho_e_pesquisa_e_le_o_total_do_cabecalho()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, Produtos, new[] { Produto(1), Produto(2) }, total: 30);
        var servico = new ProductService(api.Cliente());

        var (itens, total) = await servico.GetProductsPage(3, 12, " serra ");

        Assert.Equal(new[] { 1, 2 }, itens.Select(p => p.Id));
        Assert.Equal(30, total);
        Assert.Equal("?page=3&pageSize=12&search=serra", api.Consultas.Single());
    }

    [Fact]
    public async Task Atualizar_usa_put_no_produto_com_o_corpo()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Put, $"{Produtos}/42", Produto(42));
        var servico = new ProductService(api.Cliente());

        var produto = await servico.UpdateProduct(42, new ProductVO { Id = 42, Name = "Serra", Qntd = 4 });

        Assert.Equal(42, produto.Id);
        Assert.Equal("Serra", api.JsonDe(HttpMethod.Put, $"{Produtos}/42").RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Buscar_por_id_e_criar_vao_para_as_rotas_certas()
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, $"{Produtos}/7", Produto(7))
            .Responde(HttpMethod.Post, Produtos, Produto(8));
        var servico = new ProductService(api.Cliente());

        var buscado = await servico.GetProductById(7);
        var criado = await servico.CreateProduct(new ProductVO { Name = "Novo" });

        Assert.Equal(7, buscado.Id);
        Assert.Equal(8, criado.Id);
    }

    [Fact]
    public async Task Resposta_de_erro_vira_api_exception_com_a_mensagem_da_api()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Post, Produtos, "O preço inicial deve ser maior que zero.", HttpStatusCode.BadRequest);
        var servico = new ProductService(api.Cliente());

        var erro = await Assert.ThrowsAsync<ApiException>(() => servico.CreateProduct(new ProductVO { Name = "Novo" }));

        Assert.Equal(400, erro.StatusCode);
        Assert.Equal("O preço inicial deve ser maior que zero.", erro.Message);
    }
}
