using System.Text.Json;

namespace SmartStorage.APIGateway.Tests;

public class CoberturaDaColecaoTests
{
    private static string Gateway(params string[] rotas)
    {
        return JsonSerializer.Serialize(new
        {
            Routes = rotas.Select(rota => new
            {
                UpstreamHttpMethod = new[] { rota.Split(' ')[0] },
                UpstreamPathTemplate = rota.Split(' ')[1]
            })
        });
    }

    private static string Colecao(params string[] requisicoes)
    {
        return JsonSerializer.Serialize(new
        {
            item = new[]
            {
                new
                {
                    name = "Pasta",
                    item = requisicoes.Select(requisicao => new
                    {
                        name = requisicao,
                        request = new
                        {
                            method = requisicao.Split(' ')[0],
                            url = new { raw = "{{baseUrl}}" + requisicao.Split(' ')[1] }
                        }
                    })
                }
            }
        });
    }

    [Fact]
    public void Colecao_do_repositorio_tem_uma_requisicao_para_cada_rota_do_gateway()
    {
        var divergencias = CollectionCoverage.FindDivergences(
            RepositoryFiles.ReadFromSolutionFolder("SmartStorage.APIGateway", "appsettings.json"),
            RepositoryFiles.ReadFromSolutionFolder("..", "docs", "postman", "SmartStorage.postman_collection.json"));

        Assert.True(divergencias.Count == 0, string.Join(Environment.NewLine, divergencias));
    }

    [Fact]
    public void Requisicoes_em_pastas_que_cobrem_as_rotas_nao_divergem()
    {
        Assert.Empty(CollectionCoverage.FindDivergences(
            Gateway("GET /produtos", "POST /vendas"),
            Colecao("POST /vendas", "GET /produtos")));
    }

    [Fact]
    public void Variavel_do_gateway_e_da_colecao_contam_como_o_mesmo_trecho()
    {
        Assert.Empty(CollectionCoverage.FindDivergences(
            Gateway("PUT /produtos/{id}/estoque"),
            Colecao("PUT /produtos/{{productId}}/estoque")));
    }

    [Fact]
    public void Query_string_da_requisicao_nao_conta_no_caminho()
    {
        Assert.Empty(CollectionCoverage.FindDivergences(
            Gateway("GET /produtos"),
            Colecao("GET /produtos?page=1&pageSize=10")));
    }

    [Fact]
    public void Url_em_texto_tambem_e_lida()
    {
        var colecao = """{ "item": [ { "request": { "method": "GET", "url": "{{baseUrl}}/produtos" } } ] }""";

        Assert.Empty(CollectionCoverage.FindDivergences(Gateway("GET /produtos"), colecao));
    }

    [Fact]
    public void Comentario_no_arquivo_do_gateway_nao_atrapalha_a_leitura()
    {
        var gateway = """
            {
              "Routes": [
                // Serviço de produtos
                { "UpstreamHttpMethod": [ "GET" ], "UpstreamPathTemplate": "/produtos" }
              ]
            }
            """;

        Assert.Empty(CollectionCoverage.FindDivergences(gateway, Colecao("GET /produtos")));
    }

    [Fact]
    public void Rota_sem_requisicao_aponta_metodo_e_caminho()
    {
        var divergencias = CollectionCoverage.FindDivergences(
            Gateway("GET /produtos", "DELETE /vendas/{saleId}"),
            Colecao("GET /produtos"));

        Assert.Equal(new[] { "A rota DELETE /vendas/{saleId} não tem requisição na coleção" }, divergencias);
    }

    [Fact]
    public void Requisicao_sem_rota_aponta_metodo_e_caminho()
    {
        var divergencias = CollectionCoverage.FindDivergences(
            Gateway("GET /produtos"),
            Colecao("GET /produtos", "POST /sem-rota/{{id}}"));

        Assert.Equal(new[] { "A requisição POST /sem-rota/{{id}} não tem rota no gateway" }, divergencias);
    }

    [Fact]
    public void Mesmo_caminho_com_metodo_diferente_aponta_os_dois_lados()
    {
        var divergencias = CollectionCoverage.FindDivergences(Gateway("GET /produtos"), Colecao("POST /produtos"));

        Assert.Equal(
            new[]
            {
                "A rota GET /produtos não tem requisição na coleção",
                "A requisição POST /produtos não tem rota no gateway"
            },
            divergencias);
    }

    [Fact]
    public void Duas_requisicoes_para_a_mesma_rota_divergem()
    {
        var divergencias = CollectionCoverage.FindDivergences(
            Gateway("GET /produtos/{id}"),
            Colecao("GET /produtos/{{productId}}", "GET /produtos/{{newProductId}}"));

        Assert.Equal(new[] { "A coleção tem 2 requisições para a rota GET /produtos/{{productId}}" }, divergencias);
    }

    [Fact]
    public void Rota_com_dois_metodos_exige_uma_requisicao_por_metodo()
    {
        var gateway = """{ "Routes": [ { "UpstreamHttpMethod": [ "GET", "POST" ], "UpstreamPathTemplate": "/produtos" } ] }""";

        var divergencias = CollectionCoverage.FindDivergences(gateway, Colecao("GET /produtos"));

        Assert.Equal(new[] { "A rota POST /produtos não tem requisição na coleção" }, divergencias);
    }
}
