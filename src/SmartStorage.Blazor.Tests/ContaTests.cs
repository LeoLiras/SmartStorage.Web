using System.Net.Http;
using System.Security.Claims;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Authentication;
using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.Variables;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// O usuário logado fica no estado global durante a sessão. Ele guardava a entidade
/// User, com hash de senha e refresh token; agora guarda o UserAccountVO que o
/// my-account devolve, identificado pelo token e não por um username da tela.
/// </summary>
public class ContaTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Usuarios = "api/auth/v1";

    private (ApiFalsa Api, VariablesExtensions App) Monta()
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, $"{Usuarios}/my-account", new { id = 2, username = "usuario", fullName = "Usuário Comum", type = 0 });
        var app = new VariablesExtensions();

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton<IAuthService>(new AuthService(api.Cliente(), new AuthStateProvider(Substitute.For<IJSRuntime>()), new SessionExpiration()));
        Services.AddSingleton(app);
        Services.AddSingleton(new Dialogo(Substitute.For<IDialogService>(), new SessionExpiration()));
        AddAuthorization().SetAuthorized("usuario").SetRoles("Usuario").SetClaims(new Claim("unique_name", "usuario"));

        return (api, app);
    }

    [Fact]
    public void Conta_carrega_o_usuario_logado_pelo_my_account()
    {
        var (api, app) = Monta();

        var cut = Render<Account>();

        cut.WaitForAssertion(() => Assert.Equal("Usuário Comum", app.User.FullName));
        Assert.Equal(2, app.User.Id);
        Assert.All(api.Requisicoes, r => Assert.Equal($"{Usuarios}/my-account", r.Caminho));
        Assert.Equal("", api.Consultas.First());
    }

    [Fact]
    public void Estado_global_do_usuario_nao_tem_senha_nem_refresh_token()
    {
        var campos = typeof(VariablesExtensions).GetProperty(nameof(VariablesExtensions.User))!
            .PropertyType.GetProperties()
            .Select(p => p.Name);

        Assert.DoesNotContain(campos, c => c.Contains("Password", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(campos, c => c.Contains("RefreshToken", StringComparison.OrdinalIgnoreCase));
    }
}
