using System.Text.RegularExpressions;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// O Program.cs do WebAssembly não sobe num teste, então a cadeia é conferida no texto:
/// um serviço que fique sem o RefreshHandler derruba a sessão em 60 minutos, e um com o
/// RefreshHandler antes do SessionExpiredHandler manda ao login antes de renovar.
/// </summary>
public class CadeiaDeHandlersTests
{
    private static string ProgramCs()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);

        while (pasta is not null && !File.Exists(Path.Combine(pasta.FullName, "SmartStorageWeb.sln")))
            pasta = pasta.Parent;

        if (pasta is null)
            throw new InvalidOperationException("SmartStorageWeb.sln não encontrada acima de " + AppContext.BaseDirectory);

        return File.ReadAllText(Path.Combine(pasta.FullName, "SmartStorage.Blazor", "Program.cs"));
    }

    [Fact]
    public void Todo_servico_com_AuthHandler_renova_o_token_por_dentro_do_aviso_de_sessao()
    {
        var registros = Regex.Matches(ProgramCs(), @"AddHttpClient<(\w+),[^;]*;")
            .Select(m => (Servico: m.Groups[1].Value, Cadeia: Regex.Matches(m.Value, @"AddHttpMessageHandler<(\w+)>")
                .Select(h => h.Groups[1].Value).ToArray()))
            .Where(r => r.Cadeia.Contains("AuthHandler"))
            .ToList();

        Assert.Equal(9, registros.Count);
        Assert.All(registros, r => Assert.Equal(
            new[] { "AuthHandler", "SessionExpiredHandler", "RefreshHandler" }, r.Cadeia));
    }
}
