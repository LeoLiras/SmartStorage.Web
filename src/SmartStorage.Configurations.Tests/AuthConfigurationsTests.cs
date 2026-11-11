using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Auth;

namespace SmartStorage.Configurations.Tests;

public class AuthConfigurationsTests
{
    private static IConfiguration Configuracao(string segredo) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["TokenConfigurations:Secret"] = segredo,
            ["TokenConfigurations:Issuer"] = "Emissor",
            ["TokenConfigurations:Audience"] = "Publico",
        })
        .Build();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Segredo_ausente_derruba_o_startup_com_a_chave_na_mensagem(string segredo)
    {
        var erro = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddAuthConfiguration(Configuracao(segredo)));

        Assert.Contains("TokenConfigurations:Secret não configurado", erro.Message);
    }

    [Fact]
    public void Segredo_preenchido_registra_a_configuracao_do_token()
    {
        var servicos = new ServiceCollection().AddAuthConfiguration(Configuracao("segredo-dos-testes-com-pelo-menos-32-bytes"));

        var token = servicos.BuildServiceProvider().GetRequiredService<TokenConfiguration>();

        Assert.Equal("segredo-dos-testes-com-pelo-menos-32-bytes", token.Secret);
        Assert.Equal("Emissor", token.Issuer);
        Assert.Equal("Publico", token.Audience);
    }
}
