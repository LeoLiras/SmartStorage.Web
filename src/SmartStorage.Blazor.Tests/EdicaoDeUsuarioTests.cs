using System.Net;
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
/// própria conta. A própria conta salva pelo my-account, com a senha atual; o
/// administrador editando outro usuário envia só o UserUpdateVO, e a senha, quando
/// preenchida, vai pela rota de senha.
/// </summary>
public class EdicaoDeUsuarioTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Usuarios = "api/auth/v1";

    private IDialogService _dialogo = null!;

    private VariablesExtensions _app = null!;

    private ApiFalsa Monta(string username, string papel, long logadoId)
    {
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, $"{Usuarios}/my-account", new { id = 2, username = "usuario", fullName = "Usuário Comum", type = 0 })
            .Responde(HttpMethod.Put, $"{Usuarios}/my-account", new { id = 2, username = "usuario", fullName = "Usuário Renomeado", type = 0 })
            .Responde(HttpMethod.Get, $"{Usuarios}/3", new { id = 3, username = "maria", fullName = "Maria Souza", type = 0 })
            .Responde(HttpMethod.Post, $"{Usuarios}/update-credentials/3", new { id = 3, username = "maria", fullName = "Maria Souza", type = 1 })
            .Responde(HttpMethod.Put, $"{Usuarios}/3/password", new { id = 3, username = "maria", fullName = "Maria Souza", type = 1 });

        _dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        _dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default).ReturnsForAnyArgs(Task.FromResult(referencia));

        _app = new VariablesExtensions { User = new UserAccountVO { Id = logadoId, Username = username } };

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton<IAuthService>(new AuthService(api.Cliente(), new AuthStateProvider(Substitute.For<IJSRuntime>()), new SessionExpiration()));
        Services.AddSingleton(_app);
        Services.AddSingleton(new Dialogo(_dialogo, new SessionExpiration()));
        AddAuthorization().SetAuthorized(username).SetRoles(papel).SetClaims(new Claim("unique_name", username));

        return api;
    }

    private static void Preenche(IRenderedComponent<EditUser> cut, string rotulo, string valor)
        => cut.FindAll(".mud-input-control")
              .Single(c => c.QuerySelector("label")?.TextContent.Trim() == rotulo)
              .QuerySelector("input")!
              .Change(valor);

    private static void Atualiza(IRenderedComponent<EditUser> cut)
        => cut.FindAll("button").Single(b => b.TextContent.Contains("Atualizar")).Click();

    private string UltimaMensagem()
    {
        var chamada = _dialogo.ReceivedCalls().Last(c => c.GetMethodInfo().Name == nameof(IDialogService.ShowAsync));
        return (string)((DialogParameters)chamada.GetArguments()[1]!)["ContentText"]!;
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
        Atualiza(cut);

        cut.WaitForAssertion(() => Assert.Contains(api.Requisicoes, r => r.Metodo == HttpMethod.Post));
        var corpo = api.JsonDe(HttpMethod.Post, $"{Usuarios}/update-credentials/3").RootElement;

        Assert.Equal(new[] { "fullName", "type" }, corpo.EnumerateObject().Select(c => c.Name).OrderBy(n => n));
        Assert.Equal("Maria Souza", corpo.GetProperty("fullName").GetString());
        Assert.Equal((int)TipoUsuario.Administrador, corpo.GetProperty("type").GetInt32());
        Assert.DoesNotContain(api.Requisicoes, r => r.Caminho == $"{Usuarios}/3/password");
    }

    [Fact]
    public void Administrador_editando_outro_pede_so_a_senha_nova_e_mostra_o_login_so_para_leitura()
    {
        Monta("admin", "Administrador", logadoId: 1);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 3L));
        cut.WaitForAssertion(() => Assert.Equal("Maria Souza", cut.Instance.user.FullName));

        Assert.Equal(2, cut.FindAll("input[type=password]").Count);
        Assert.DoesNotContain("Senha atual", cut.Markup);
        var login = cut.FindAll("input").Single(i => i.GetAttribute("value") == "maria");
        Assert.True(login.HasAttribute("readonly"));
    }

    [Fact]
    public void Senha_preenchida_pelo_administrador_vai_so_pela_rota_de_senha()
    {
        var api = Monta("admin", "Administrador", logadoId: 1);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 3L));
        cut.WaitForAssertion(() => Assert.Equal("Maria Souza", cut.Instance.user.FullName));

        Preenche(cut, "Nova senha", "NovaSenha1");
        Preenche(cut, "Confirmar nova senha", "NovaSenha1");
        Atualiza(cut);

        cut.WaitForAssertion(() => Assert.Contains(api.Requisicoes, r => r.Caminho == $"{Usuarios}/3/password"));
        var senha = api.JsonDe(HttpMethod.Put, $"{Usuarios}/3/password").RootElement;
        Assert.Equal(new[] { "newPassword" }, senha.EnumerateObject().Select(c => c.Name));
        Assert.Equal("NovaSenha1", senha.GetProperty("newPassword").GetString());

        var edicao = api.JsonDe(HttpMethod.Post, $"{Usuarios}/update-credentials/3").RootElement;
        Assert.Equal(new[] { "fullName", "type" }, edicao.EnumerateObject().Select(c => c.Name).OrderBy(n => n));
    }

    [Fact]
    public void Propria_conta_salva_pelo_my_account_com_a_senha_atual()
    {
        var api = Monta("usuario", "Usuario", logadoId: 2);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 2L));
        cut.WaitForAssertion(() => Assert.Equal("Usuário Comum", cut.Instance.user.FullName));

        Assert.Empty(cut.FindAll("input[type=radio]"));

        Preenche(cut, "Nome Completo", "Usuário Renomeado");
        Preenche(cut, "Senha atual", "usuario123");
        Preenche(cut, "Nova senha", "NovaSenha1");
        Preenche(cut, "Confirmar nova senha", "NovaSenha1");
        Atualiza(cut);

        cut.WaitForAssertion(() => Assert.Contains(api.Requisicoes, r => r.Metodo == HttpMethod.Put));
        var corpo = api.JsonDe(HttpMethod.Put, $"{Usuarios}/my-account").RootElement;

        Assert.Equal(new[] { "currentPassword", "fullName", "newPassword" }, corpo.EnumerateObject().Select(c => c.Name).OrderBy(n => n));
        Assert.Equal("Usuário Renomeado", corpo.GetProperty("fullName").GetString());
        Assert.Equal("usuario123", corpo.GetProperty("currentPassword").GetString());
        Assert.Equal("NovaSenha1", corpo.GetProperty("newPassword").GetString());
        Assert.DoesNotContain(api.Requisicoes, r => r.Caminho.Contains("update-credentials") || r.Caminho.EndsWith("/password"));
        cut.WaitForAssertion(() => Assert.Equal("Usuário Renomeado", _app.User.FullName));
    }

    [Fact]
    public void Propria_conta_sem_a_senha_atual_nao_envia()
    {
        var api = Monta("usuario", "Usuario", logadoId: 2);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 2L));
        cut.WaitForAssertion(() => Assert.Equal("Usuário Comum", cut.Instance.user.FullName));

        Atualiza(cut);

        cut.WaitForAssertion(() => Assert.Contains("A senha atual é obrigatória!", cut.Markup));
        Assert.DoesNotContain(api.Requisicoes, r => r.Metodo == HttpMethod.Put);
    }

    [Fact]
    public void Erro_da_api_na_propria_conta_aparece_no_dialogo()
    {
        var api = Monta("usuario", "Usuario", logadoId: 2);
        api.Responde(HttpMethod.Put, $"{Usuarios}/my-account", "A senha atual está incorreta.", HttpStatusCode.BadRequest);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 2L));
        cut.WaitForAssertion(() => Assert.Equal("Usuário Comum", cut.Instance.user.FullName));

        Preenche(cut, "Senha atual", "errada");
        Atualiza(cut);

        cut.WaitForAssertion(() => Assert.Contains("A senha atual está incorreta.", UltimaMensagem()));
        Assert.Null(_app.User.FullName);
    }

    [Fact]
    public void Erro_da_api_na_rota_de_senha_aparece_no_dialogo()
    {
        var api = Monta("admin", "Administrador", logadoId: 1);
        api.Responde(HttpMethod.Put, $"{Usuarios}/3/password", "A nova senha é obrigatória.", HttpStatusCode.BadRequest);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 3L));
        cut.WaitForAssertion(() => Assert.Equal("Maria Souza", cut.Instance.user.FullName));

        Preenche(cut, "Nova senha", "NovaSenha1");
        Preenche(cut, "Confirmar nova senha", "NovaSenha1");
        Atualiza(cut);

        cut.WaitForAssertion(() => Assert.Contains("A nova senha é obrigatória.", UltimaMensagem()));
    }

    [Fact]
    public void Confirmacao_diferente_nao_envia_a_senha()
    {
        var api = Monta("admin", "Administrador", logadoId: 1);

        var cut = Render<EditUser>(p => p.Add(c => c.UserId, 3L));
        cut.WaitForAssertion(() => Assert.Equal("Maria Souza", cut.Instance.user.FullName));

        Preenche(cut, "Nova senha", "NovaSenha1");
        Preenche(cut, "Confirmar nova senha", "Outra1234");
        Atualiza(cut);

        cut.WaitForAssertion(() => Assert.Contains("As senhas não conferem", cut.Markup));
        Assert.DoesNotContain(api.Requisicoes, r => r.Metodo != HttpMethod.Get);
    }
}
