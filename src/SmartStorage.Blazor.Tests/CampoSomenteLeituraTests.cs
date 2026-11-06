using System.Text.RegularExpressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// Estilo de campo só leitura da issue #48 (T-48.01): os seletores de
/// `.app-readonly` no app.css dependem da estrutura que o MudBlazor renderiza.
/// Se uma atualização dele mudar essa estrutura, o CSS para de valer sem erro
/// nenhum, e só este teste avisa.
/// </summary>
public class CampoSomenteLeituraTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private static readonly string AppCss = File.ReadAllText(Path.Combine(LocalizaWwwroot(), "css", "app.css"));

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

    private static List<string> SeletoresDoCss() => Regex
        .Matches(Regex.Replace(AppCss, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline), @"([^{}]+)\{")
        .SelectMany(m => m.Groups[1].Value.Split(','))
        .Select(s => s.Trim())
        .ToList();

    // Só os seletores de campo (a moldura da imagem tem regra própria). Os
    // estados (hover, foco) não existem no DOM do bUnit; sem eles, o seletor
    // ainda prova que a estrutura que ele percorre existe.
    private static List<string> SeletoresDoSomenteLeitura() => SeletoresDoCss()
        .Where(s => s.Contains(".app-readonly") && s.Contains(".mud-input"))
        .Select(s => Regex.Replace(s, @":not\([^)]*\)|:(hover|focus-within)", string.Empty))
        .ToList();

    private IRenderedComponent<IComponent> RenderizaCampo(string campo, string classe)
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();

        RenderFragment fragmento = campo switch
        {
            "texto" => b =>
            {
                b.OpenComponent<MudTextField<string>>(0);
                b.AddAttribute(1, "Label", "Prateleira atual");
                b.AddAttribute(2, "Value", "Prateleira A1");
                b.AddAttribute(3, "ReadOnly", true);
                b.AddAttribute(4, "Variant", Variant.Outlined);
                b.AddAttribute(5, "Class", classe);
                b.CloseComponent();
            },
            "numero" => b =>
            {
                b.OpenComponent<MudNumericField<int>>(0);
                b.AddAttribute(1, "Label", "Quantidade na prateleira");
                b.AddAttribute(2, "Value", 12);
                b.AddAttribute(3, "ReadOnly", true);
                b.AddAttribute(4, "Variant", Variant.Outlined);
                b.AddAttribute(5, "Class", classe);
                b.CloseComponent();
            },
            "selecao" => b =>
            {
                b.OpenComponent<MudPopoverProvider>(0);
                b.CloseComponent();
                b.OpenComponent<MudSelect<int>>(1);
                b.AddAttribute(2, "Label", "Responsável");
                b.AddAttribute(3, "ReadOnly", true);
                b.AddAttribute(4, "Variant", Variant.Outlined);
                b.AddAttribute(5, "Class", classe);
                b.CloseComponent();
            },
            "data" => b =>
            {
                b.OpenComponent<MudPopoverProvider>(0);
                b.CloseComponent();
                b.OpenComponent<MudDatePicker>(1);
                b.AddAttribute(2, "Label", "Data da venda");
                b.AddAttribute(3, "ReadOnly", true);
                b.AddAttribute(4, "Variant", Variant.Outlined);
                b.AddAttribute(5, "Class", classe);
                b.CloseComponent();
            },
            _ => throw new ArgumentOutOfRangeException(nameof(campo)),
        };

        return Render(fragmento);
    }

    [Fact]
    public void App_css_declara_o_estilo_so_pela_classe_e_nao_pelo_atributo_readonly()
    {
        Assert.NotEmpty(SeletoresDoSomenteLeitura());
        Assert.DoesNotContain(SeletoresDoCss(), s => s.Contains("[readonly]"));
    }

    [Theory]
    [InlineData("texto")]
    [InlineData("numero")]
    [InlineData("selecao")]
    [InlineData("data")]
    public void Cada_seletor_do_estilo_encontra_o_campo_que_tem_a_classe(string campo)
    {
        var cut = RenderizaCampo(campo, "app-readonly");
        var seletores = SeletoresDoSomenteLeitura()
            .Where(s => campo == "numero" || !s.Contains("mud-input-numeric-spin"));

        Assert.All(seletores, seletor =>
            Assert.True(cut.FindAll(seletor).Count == 1, $"\"{seletor}\" não encontrou um elemento no campo de {campo}"));
    }

    [Theory]
    [InlineData("texto")]
    [InlineData("selecao")]
    public void Campo_readonly_sem_a_classe_fica_com_o_estilo_do_editavel(string campo)
    {
        var cut = RenderizaCampo(campo, null);

        Assert.All(SeletoresDoSomenteLeitura(), seletor => Assert.Empty(cut.FindAll(seletor)));
    }
}
