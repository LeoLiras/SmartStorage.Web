using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Authentication;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.Variables;
using SmartStorage.Shared.VO;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class LoginTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private IAuthService Monta()
    {
        var auth = Substitute.For<IAuthService>();

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(auth);
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(Substitute.For<IDialogService>(), new SessionExpiration()));

        return auth;
    }

    private static void Digita(IRenderedComponent<Login> cut, string rotulo, string valor)
        => cut.FindAll("label").First(l => l.TextContent.Trim() == rotulo)
              .Closest(".mud-input-control")!.QuerySelector("input")!.Input(valor);

    [Fact]
    public void Enviar_o_formulario_faz_login_com_o_que_foi_digitado()
    {
        var auth = Monta();
        var cut = Render<Login>();

        Digita(cut, "Login", "admin");
        Digita(cut, "Senha", "Senha123");
        cut.Find("form").Submit();

        auth.Received(1).Login(Arg.Is<UserVO>(u => u.Username == "admin" && u.Password == "Senha123"));
    }

    [Fact]
    public void Formulario_sem_senha_nao_chama_o_login()
    {
        var auth = Monta();
        var cut = Render<Login>();

        Digita(cut, "Login", "admin");
        cut.Find("form").Submit();

        auth.DidNotReceiveWithAnyArgs().Login(default!);
        Assert.Equal("Entrar", cut.Find("button[type=submit]").TextContent.Trim());
    }
}
