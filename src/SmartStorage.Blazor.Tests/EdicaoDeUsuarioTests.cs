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
using SmartStorage.Shared.Enum;
using SmartStorage.Shared.VO;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// A tela Conta embute o EditUser com o id de quem está logado. Depois que a busca
/// por id passou a exigir administrador, o usuário comum recebia 403 ao abrir a
/// própria conta. A edição envia só o UserUpdateVO: a senha tem rota própria, e o
/// username não muda.
/// </summary>
public class EdicaoDeUsuarioTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Usuarios = "api/auth/v1";

    private ApiFalsa Monta(string username, string papel, long logadoId)
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, $"{Usuarios}/my-account", new { id = 2, username = "usuario", fullName = "Usuário Comum", type = 0 })
            .Responde(HttpMethod.Get, $"{Usuarios}/3", new { id = 3, username = "maria", fullName = "Maria Souza", type = 0 })
            .Responde(HttpMethod.Post, $"{Usuarios}/update-credentials/3", new { id = 3, username = "maria", fullName = "Maria Souza", type = 1 });

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton<IAuthService>(new AuthService(api.Cliente(), new AuthStateProvider(Substitute.For<IJSRuntime>()), new SessionExpiration()));
        Services.AddSingleton(new VariablesExtensions { User = new UserAccountVO { Id = logadoId, Username = username } });
        Services.AddSingleton(new Dialogo(Substitute.For<IDialogService>(), new SessionExpiration()));
        AddAuthorization().SetAuthorized(username).SetRoles(papel).SetClaims(new Claim("unique_name", username));

        return api;
    }

    [Fact]
    public void Usuario_comum_abre_a_propria_conta_pelo_my_account()
    {
        var api = Monta("usuario", "Usuario", logadoId: 2);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 2L));

        cut.WaitForAssertion(() => Assert.Equal("Usuário Comum", cut.Instance.user.FullName));
        Assert.Contains(api.Requisicoes, r => r.Caminho == $"{Usuarios}/my-account");
        Assert.DoesNotContain(api.Requisicoes, r => r.Caminho == $"{Usuarios}/2");
    }

    [Fact]
    public void Administrador_abre_outro_usuario_pela_busca_por_id()
    {
        var api = Monta("admin", "Administrador", logadoId: 1);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 3L));

        cut.WaitForAssertion(() => Assert.Equal("Maria Souza", cut.Instance.user.FullName));
        Assert.Contains(api.Requisicoes, r => r.Caminho == $"{Usuarios}/3");
        Assert.DoesNotContain(api.Requisicoes, r => r.Caminho == $"{Usuarios}/my-account");
    }

    [Fact]
    public void Edicao_envia_so_nome_completo_e_papel_para_o_id_na_rota()
    {
        var api = Monta("admin", "Administrador", logadoId: 1);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 3L));
        cut.WaitForAssertion(() => Assert.Equal("Maria Souza", cut.Instance.user.FullName));

        cut.Instance.user.Type = TipoUsuario.Administrador;
        cut.FindAll("button").Single(b => b.TextContent.Contains("Atualizar")).Click();

        cut.WaitForAssertion(() => Assert.Contains(api.Requisicoes, r => r.Metodo == HttpMethod.Post));
        var corpo = api.JsonDe(HttpMethod.Post, $"{Usuarios}/update-credentials/3").RootElement;

        Assert.Equal(new[] { "fullName", "type" }, corpo.EnumerateObject().Select(c => c.Name).OrderBy(n => n));
        Assert.Equal("Maria Souza", corpo.GetProperty("fullName").GetString());
        Assert.Equal((int)TipoUsuario.Administrador, corpo.GetProperty("type").GetInt32());
    }

    [Fact]
    public void Tela_nao_pede_senha_e_mostra_o_login_so_para_leitura()
    {
        Monta("admin", "Administrador", logadoId: 1);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 3L));
        cut.WaitForAssertion(() => Assert.Equal("Maria Souza", cut.Instance.user.FullName));

        Assert.Empty(cut.FindAll("input[type=password]"));
        var login = cut.FindAll("input").Single(i => i.GetAttribute("value") == "maria");
        Assert.True(login.HasAttribute("readonly"));
    }
}
