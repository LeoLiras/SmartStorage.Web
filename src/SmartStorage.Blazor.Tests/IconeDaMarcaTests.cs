using System.Text.RegularExpressions;
using Bunit;
using SmartStorage.Blazor.Layout;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// Icone proprio da issue #47: a barra superior e o login mostram o mesmo
/// favicon.svg da aba, sem quadrado de fundo, e o index.html nao aponta mais
/// para os icones do template do Blazor.
/// </summary>
public class IconeDaMarcaTests : BunitContext
{
    private static readonly string Wwwroot = LocalizaWwwroot();

    private static string LocalizaWwwroot()
    {
        for (var pasta = new DirectoryInfo(AppContext.BaseDirectory); pasta is not null; pasta = pasta.Parent)
        {
            var candidato = Path.Combine(pasta.FullName, "SmartStorage.Blazor", "wwwroot");

            if (File.Exists(Path.Combine(candidato, "index.html")))
                return candidato;
        }

        throw new DirectoryNotFoundException("wwwroot do SmartStorage.Blazor não encontrado acima da pasta dos testes.");
    }

    private static string IndexHtml => File.ReadAllText(Path.Combine(Wwwroot, "index.html"));

    private static List<(string Rel, string Type, string Href)> LinksDeIcone() => Regex
        .Matches(IndexHtml, "<link[^>]*rel=\"(icon|apple-touch-icon)\"[^>]*>")
        .Select(m => (
            m.Groups[1].Value,
            Regex.Match(m.Value, "type=\"([^\"]+)\"").Groups[1].Value,
            Regex.Match(m.Value, "href=\"([^\"]+)\"").Groups[1].Value))
        .ToList();

    [Fact]
    public void Marca_da_barra_superior_usa_o_favicon_svg_com_a_classe_recebida()
    {
        var cut = Render<Brand>(p => p.Add(b => b.Class, "ml-2"));

        var marca = cut.Find("span.brand");
        Assert.Contains("ml-2", marca.ClassList);
        Assert.Equal("Smart Storage", marca.TextContent.Trim());

        var icone = marca.QuerySelector("img.brand-mark")!;
        Assert.Equal("favicon.svg", icone.GetAttribute("src"));
        Assert.Equal(string.Empty, icone.GetAttribute("alt"));
    }

    [Fact]
    public void Favicon_svg_nao_tem_o_quadrado_de_fundo()
    {
        Assert.DoesNotContain("<rect", File.ReadAllText(Path.Combine(Wwwroot, "favicon.svg")));
    }

    [Fact]
    public void Aba_tem_o_titulo_e_os_icones_da_marca()
    {
        Assert.Contains("<title>Smart Storage</title>", IndexHtml);

        var links = LinksDeIcone();
        Assert.Contains(("icon", "image/svg+xml", "favicon.svg"), links);
        Assert.Contains(("icon", "image/png", "favicon.png"), links);
        Assert.Contains(links, l => l.Rel == "apple-touch-icon" && l.Href == "apple-touch-icon.png");
    }

    [Fact]
    public void Todo_icone_do_index_existe_e_o_do_template_saiu()
    {
        Assert.All(LinksDeIcone(), l => Assert.True(File.Exists(Path.Combine(Wwwroot, l.Href)), $"{l.Href} não existe no wwwroot"));
        Assert.False(File.Exists(Path.Combine(Wwwroot, "icon-192.png")));
        Assert.DoesNotContain("icon-192", IndexHtml);
    }
}
