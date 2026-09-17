using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Enums;
using SmartStorage.Blazor.Pages.Loading;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class CarregamentoEDialogoTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    [Fact]
    public void Carregamento_mostra_a_mensagem_padrao_e_o_esqueleto_sem_gif()
    {
        var cut = Render<Loading>();

        Assert.Contains("Carregando...", cut.Find("[role=status]").TextContent);
        Assert.Equal(4, cut.FindAll(".app-skeleton-lines .sk").Count);
        Assert.Empty(cut.FindAll("img"));
    }

    [Theory]
    [InlineData(ELoadingShape.Table, ".app-skeleton-row", 5)]
    [InlineData(ELoadingShape.Cards, ".app-skeleton-card", 4)]
    [InlineData(ELoadingShape.Form, ".app-skeleton-field", 3)]
    public void Carregamento_desenha_o_formato_da_tela(ELoadingShape formato, string seletor, int quantidade)
    {
        var cut = Render<Loading>(p => p
            .Add(x => x.Shape, formato)
            .Add(x => x.Message, "Carregando vendas..."));

        Assert.Contains("Carregando vendas...", cut.Find("[role=status]").TextContent);
        Assert.Equal(quantidade, cut.FindAll(seletor).Count);
    }

    private (IRenderedComponent<MudDialogProvider> Provedor, Dialogo Dialogo) MontaDialogo()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        var provedor = Render<MudDialogProvider>();
        var dialogo = new Dialogo(Services.GetRequiredService<IDialogService>(), new SessionExpiration());
        return (provedor, dialogo);
    }

    [Fact]
    public async Task Dialogo_de_erro_usa_a_cor_de_erro_e_confirma_no_sim()
    {
        var (provedor, dialogo) = MontaDialogo();

        var resposta = provedor.InvokeAsync(() => dialogo.ShowDialogAsync("Não foi possível salvar.", "Erro", state: EDialogStates.Error, showCancel: true, showYes: true));

        var janela = provedor.WaitForElement(".app-dialog-error");
        Assert.Contains("Não foi possível salvar.", janela.TextContent);
        var botoes = provedor.FindAll(".app-dialog-error button").Select(b => b.TextContent.Trim()).ToList();
        Assert.Equal(new[] { "Cancelar", "Sim" }, botoes);

        provedor.FindAll(".app-dialog-error button").Single(b => b.TextContent.Trim() == "Sim").Click();

        Assert.True(await resposta);
        provedor.WaitForAssertion(() => Assert.Empty(provedor.FindAll(".app-dialog")));
    }

    [Fact]
    public async Task Cancelar_fecha_o_dialogo_sem_confirmar()
    {
        var (provedor, dialogo) = MontaDialogo();

        var resposta = provedor.InvokeAsync(() => dialogo.ShowDialogAsync("Desfazer a alocação?", "Atenção", state: EDialogStates.Warning, showCancel: true, showYes: true));

        provedor.WaitForElement(".app-dialog-warning");
        provedor.FindAll(".app-dialog-warning button").Single(b => b.TextContent.Trim() == "Cancelar").Click();

        Assert.False(await resposta);
    }

    [Fact]
    public void Dialogo_sem_estado_informado_e_de_sucesso_com_ok()
    {
        var (provedor, dialogo) = MontaDialogo();

        _ = provedor.InvokeAsync(() => dialogo.ShowDialogAsync("Produto criado."));

        provedor.WaitForElement(".app-dialog-success");
        var botoes = provedor.FindAll(".app-dialog-success button").Select(b => b.TextContent.Trim()).ToList();
        Assert.Equal(new[] { "Ok" }, botoes);
    }
}
