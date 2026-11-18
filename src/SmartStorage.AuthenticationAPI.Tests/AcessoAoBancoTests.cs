namespace SmartStorage.AuthenticationAPI.Tests;

public class AcessoAoBancoTests
{
    // Lê o código versionado, e não o assembly, porque SQL em texto não deixa rastro que a reflexão veja.
    private static IEnumerable<string> ArquivosDaApi()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);

        while (pasta is not null && !File.Exists(Path.Combine(pasta.FullName, "SmartStorageWeb.sln")))
            pasta = pasta.Parent;

        Assert.NotNull(pasta);

        var projeto = Path.Combine(pasta.FullName, "SmartStorage.AuthenticationAPI");
        var saida = new[] { $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}" };

        return Directory.EnumerateFiles(projeto, "*.cs", SearchOption.AllDirectories)
            .Where(arquivo => !saida.Any(arquivo.Contains));
    }

    [Theory]
    [InlineData("FromSqlRaw")]
    [InlineData("ExecuteSqlRaw")]
    [InlineData("SqlQueryRaw")]
    [InlineData("CommandText")]
    public void Nenhum_codigo_da_AuthenticationAPI_executa_SQL_recebido_como_texto(string chamada)
    {
        var comChamada = ArquivosDaApi()
            .Where(arquivo => File.ReadAllText(arquivo).Contains(chamada))
            .Select(Path.GetFileName);

        Assert.Empty(comChamada);
    }
}
