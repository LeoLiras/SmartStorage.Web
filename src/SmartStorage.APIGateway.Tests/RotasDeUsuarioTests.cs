namespace SmartStorage.APIGateway.Tests;

public class RotasDeUsuarioTests
{
    [Theory]
    [InlineData("appsettings.json")]
    [InlineData("appsettings.Docker.json")]
    public void Gateway_expoe_a_minha_conta_e_nao_a_busca_por_username(string arquivo)
    {
        var conteudo = RepositoryFiles.ReadFromSolutionFolder("SmartStorage.APIGateway", arquivo);

        Assert.Contains("\"UpstreamPathTemplate\": \"/api/auth/v1/my-account\"", conteudo);
        Assert.Contains("\"DownstreamPathTemplate\": \"/api/auth/v1/my-account\"", conteudo);
        Assert.DoesNotContain("user-by-username", conteudo);
    }
}
