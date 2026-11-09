namespace SmartStorage.APIGateway.Tests;

public static class RepositoryFiles
{
    // Sobe das pastas de saída até a pasta da solution, porque os testes conferem
    // os arquivos versionados e não uma cópia feita no build.
    public static string ReadFromSolutionFolder(params string[] relativePath)
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);

        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "SmartStorageWeb.sln")))
        {
            folder = folder.Parent;
        }

        Assert.NotNull(folder);

        return File.ReadAllText(Path.Combine([folder.FullName, .. relativePath]));
    }
}
