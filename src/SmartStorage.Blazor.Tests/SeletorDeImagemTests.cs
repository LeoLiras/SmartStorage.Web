using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Utils.Variables;

namespace SmartStorage.Blazor.Tests;

public class SeletorDeImagemTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private IRenderedComponent<ProductImagePicker> Renderiza(bool somenteLeitura = false)
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(new VariablesExtensions());

        return Render<ProductImagePicker>(parametros => parametros
            .Add(p => p.AltText, "Imagem do produto")
            .Add(p => p.ReadOnly, somenteLeitura));
    }

    private static IElement LinhaDeBotoes(IRenderedComponent<ProductImagePicker> cut) =>
        cut.FindAll("button").First(b => b.TextContent.Contains("Limpar")).ParentElement!;

    [Fact]
    public void Botoes_TemMargemSobreAMoldura()
    {
        var cut = Renderiza();

        var linha = LinhaDeBotoes(cut);

        Assert.Single(cut.Nodes.OfType<IElement>());
        Assert.Contains("app-image-picker", linha.ParentElement!.ClassList);
        Assert.Contains("mt-4", linha.ClassList);
        Assert.True(linha.ParentElement.QuerySelector(".app-form-image")!.CompareDocumentPosition(linha).HasFlag(DocumentPositions.Following));
    }

    [Fact]
    public void Botoes_ComNomeDoArquivo_ContinuamComMargem()
    {
        var cut = Renderiza();

        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(Png, "capacete.png", contentType: "image/png"));

        var nome = cut.WaitForElement(".app-muted");
        var linha = LinhaDeBotoes(cut);

        Assert.Equal("capacete.png", nome.TextContent.Trim());
        Assert.Contains("mt-2", nome.ClassList);
        Assert.Contains("mt-4", linha.ClassList);
        Assert.True(nome.CompareDocumentPosition(linha).HasFlag(DocumentPositions.Following));
    }

    [Fact]
    public void SomenteLeitura_NaoMostraBotoes()
    {
        var cut = Renderiza(somenteLeitura: true);

        Assert.Empty(cut.FindAll("button"));
        Assert.Single(cut.FindAll(".app-image-picker .app-form-image"));
    }
}
