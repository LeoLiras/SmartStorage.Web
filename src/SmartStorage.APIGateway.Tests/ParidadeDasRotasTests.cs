namespace SmartStorage.APIGateway.Tests;

public class ParidadeDasRotasTests
{
    private static string Arquivo(params string[] rotas)
    {
        var itens = rotas.Select(rota =>
        {
            var partes = rota.Split(' ');

            return $$"""{ "UpstreamHttpMethod": [ "{{partes[0]}}" ], "UpstreamPathTemplate": "{{partes[1]}}" }""";
        });

        return $$"""{ "Routes": [ {{string.Join(",", itens)}} ] }""";
    }

    [Fact]
    public void Arquivos_do_gateway_declaram_as_mesmas_rotas_na_mesma_ordem()
    {
        var divergencias = RouteParity.FindDivergences(
            RepositoryFiles.ReadFromSolutionFolder("SmartStorage.APIGateway", "appsettings.json"),
            RepositoryFiles.ReadFromSolutionFolder("SmartStorage.APIGateway", "appsettings.Docker.json"));

        Assert.True(divergencias.Count == 0, string.Join(Environment.NewLine, divergencias));
    }

    [Fact]
    public void Rotas_iguais_na_mesma_ordem_nao_divergem()
    {
        var arquivo = Arquivo("GET /produtos", "POST /vendas");

        Assert.Empty(RouteParity.FindDivergences(arquivo, arquivo));
    }

    [Fact]
    public void Comentario_no_arquivo_nao_atrapalha_a_leitura()
    {
        var comentado = """
            {
              "Routes": [
                // Serviço de produtos
                { "UpstreamHttpMethod": [ "GET" ], "UpstreamPathTemplate": "/produtos" }
              ]
            }
            """;

        Assert.Empty(RouteParity.FindDivergences(comentado, Arquivo("GET /produtos")));
    }

    [Fact]
    public void Duas_rotas_trocadas_de_lugar_apontam_as_duas_posicoes()
    {
        var divergencias = RouteParity.FindDivergences(
            Arquivo("GET /produtos", "POST /vendas", "GET /prateleiras"),
            Arquivo("POST /vendas", "GET /produtos", "GET /prateleiras"));

        Assert.Equal(
            new[]
            {
                "Posição 1: appsettings.json tem GET /produtos, appsettings.Docker.json tem POST /vendas",
                "Posição 2: appsettings.json tem POST /vendas, appsettings.Docker.json tem GET /produtos"
            },
            divergencias);
    }

    [Fact]
    public void Rota_que_falta_em_um_arquivo_aponta_a_posicao()
    {
        var divergencias = RouteParity.FindDivergences(
            Arquivo("GET /produtos", "POST /vendas"),
            Arquivo("GET /produtos"));

        Assert.Equal(
            new[] { "Posição 2: appsettings.json tem POST /vendas, appsettings.Docker.json tem nenhuma rota" },
            divergencias);
    }

    [Fact]
    public void Mesmo_caminho_com_metodo_diferente_diverge()
    {
        var divergencias = RouteParity.FindDivergences(Arquivo("GET /produtos"), Arquivo("POST /produtos"));

        Assert.Single(divergencias);
    }

    [Fact]
    public void Ordem_dos_metodos_na_mesma_rota_nao_diverge()
    {
        var doisMetodos = """{ "Routes": [ { "UpstreamHttpMethod": [ "GET", "POST" ], "UpstreamPathTemplate": "/a" } ] }""";
        var invertidos = """{ "Routes": [ { "UpstreamHttpMethod": [ "POST", "GET" ], "UpstreamPathTemplate": "/a" } ] }""";

        Assert.Empty(RouteParity.FindDivergences(doisMetodos, invertidos));
    }
}
