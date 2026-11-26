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
using SmartStorage.Shared.Validation;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// O cadastro tinha uma regra de senha própria (maiúscula, minúscula e número),
/// diferente da que a API passou a exigir: recusava senha que a API aceita e deixava
/// passar senha sem caractere especial. A tela agora usa a PasswordPolicy do Shared.
/// </summary>
public class CadastroDeUsuarioTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string Cadastro = "api/auth/v1/create";

    private readonly IDialogService _dialogo = Substitute.For<IDialogService>();

    private readonly LocalStorageFalso _storage = new();

    private ApiFalsa Monta(Func<ApiFalsa, ApiFalsa>? respostaDoCadastro = null)
    {
        respostaDoCadastro ??= a => a.Responde(HttpMethod.Post, Cadastro, new { fullname = "Maria Souza", username = "maria", type = 0 });
        var api = respostaDoCadastro(new ApiFalsa());

        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        _dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default).ReturnsForAnyArgs(Task.FromResult(referencia));

        _storage.Itens[AuthStateProvider.tokenKey] = "token-do-admin";
        _storage.Itens[AuthStateProvider.refreshTokenKey] = "refresh-do-admin";

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton<IAuthService>(new AuthService(api.Cliente(), new AuthStateProvider(_storage), new SessionExpiration()));
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(_dialogo, new SessionExpiration()));
        AddAuthorization().SetAuthorized("admin").SetRoles("Administrador").SetClaims(new Claim("unique_name", "admin"));

        return api;
    }

    private static void Preenche(IRenderedComponent<Register> cut, string rotulo, string valor)
        => cut.FindAll(".mud-input-control")
              .Single(c => c.QuerySelector("label")?.TextContent.Trim() == rotulo)
              .QuerySelector("input")!
              .Change(valor);

    private static void Cadastra(IRenderedComponent<Register> cut, string senha, string confirmacao)
    {
        Preenche(cut, "Nome Completo", "Maria Souza");
        Preenche(cut, "Login", "maria");
        Preenche(cut, "Senha", senha);
        Preenche(cut, "Confirmar Senha", confirmacao);
        cut.FindAll("button").Single(b => b.TextContent.Contains("Cadastrar")).Click();
    }

    [Fact]
    public void Senha_mostra_a_regra_da_api_como_ajuda()
    {
        Monta();

        var cut = Render<Register>();

        Assert.Contains(PasswordPolicy.RuleMessage, cut.Markup);
        Assert.DoesNotContain("maiúscula", cut.Markup);
    }

    [Fact]
    public void Senha_sem_maiuscula_que_cumpre_a_regra_e_enviada()
    {
        var api = Monta();

        var cut = Render<Register>();
        Cadastra(cut, "nova@senha1", "nova@senha1");

        cut.WaitForAssertion(() => Assert.Contains(api.Requisicoes, r => r.Caminho == Cadastro));
        Assert.Equal("nova@senha1", api.JsonDe(HttpMethod.Post, Cadastro).RootElement.GetProperty("password").GetString());
    }

    /// <summary>
    /// O cadastro aceito fazia logout: o administrador que acabava de cadastrar alguém
    /// perdia a sessão e caía na tela inicial, sem ver o usuário novo na Conta.
    /// </summary>
    [Fact]
    public void Cadastro_aceito_mantem_a_sessao_e_leva_a_conta()
    {
        var api = Monta();

        var cut = Render<Register>();
        Cadastra(cut, "nova@senha1", "nova@senha1");

        cut.WaitForAssertion(() => Assert.Contains(_dialogo.ReceivedCalls(), c => c.GetMethodInfo().Name == nameof(IDialogService.ShowAsync)));

        var chamada = _dialogo.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IDialogService.ShowAsync));
        var parametros = (DialogParameters)chamada.GetArguments()[1]!;
        Assert.Equal("Cadastro realizado com sucesso.", (string)parametros["ContentText"]!);
        Assert.Equal("user/account", (string)parametros["Navigate"]!);

        Assert.Equal("token-do-admin", _storage.Itens[AuthStateProvider.tokenKey]);
        Assert.Equal("refresh-do-admin", _storage.Itens[AuthStateProvider.refreshTokenKey]);
        Assert.DoesNotContain(api.Requisicoes, r => r.Caminho == "api/auth/v1/revoke");
    }

    /// <summary>
    /// O serviço trocava qualquer recusa por "Erro ao criar usuário X.": o administrador
    /// não sabia se o login já existia ou se a senha feria a regra.
    /// </summary>
    [Fact]
    public void Recusa_da_api_mostra_o_motivo_no_dialogo()
    {
        var api = Monta(a => a.Responde(HttpMethod.Post, Cadastro, "Já existe um usuário com esse nome.", HttpStatusCode.BadRequest));

        var cut = Render<Register>();
        Cadastra(cut, "nova@senha1", "nova@senha1");

        cut.WaitForAssertion(() => Assert.Contains(_dialogo.ReceivedCalls(), c => c.GetMethodInfo().Name == nameof(IDialogService.ShowAsync)));

        var chamada = _dialogo.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IDialogService.ShowAsync));
        var parametros = (DialogParameters)chamada.GetArguments()[1]!;
        Assert.Contains("Já existe um usuário com esse nome.", (string)parametros["ContentText"]!);
        Assert.DoesNotContain("Erro ao criar usuário", (string)parametros["ContentText"]!);
        Assert.Equal("token-do-admin", _storage.Itens[AuthStateProvider.tokenKey]);
    }

    [Fact]
    public void Senha_sem_caractere_especial_nao_envia_e_mostra_a_mensagem()
    {
        var api = Monta();

        var cut = Render<Register>();
        Cadastra(cut, "NovaSenha1", "NovaSenha1");

        cut.WaitForAssertion(() => Assert.Contains("mud-input-error", cut.Markup));
        Assert.Contains(PasswordPolicy.RuleMessage, cut.Find(".mud-input-error").TextContent);
        Assert.Empty(api.Requisicoes);
    }

    [Fact]
    public void Confirmacao_diferente_nao_envia()
    {
        var api = Monta();

        var cut = Render<Register>();
        Cadastra(cut, "nova@senha1", "outra@senha1");

        cut.WaitForAssertion(() => Assert.Contains("As senhas não conferem", cut.Markup));
        Assert.Empty(api.Requisicoes);
    }
}
