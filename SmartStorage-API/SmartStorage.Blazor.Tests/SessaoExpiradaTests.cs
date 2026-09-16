using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages.Authentication;
using SmartStorage.Blazor.Pages.Product;
using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using SmartStorage.Shared.VO;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class SessaoExpiradaTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private const string CaminhoProduto = "api/storage/products/v1/42";

    private static HttpClient Cliente(ApiFalsa api, SessionExpiration sessao, bool comToken = true)
    {
        var http = new HttpClient(new SessionExpiredHandler(sessao) { InnerHandler = api })
        {
            BaseAddress = new Uri("http://localhost/"),
        };

        if (comToken)
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-vencido");

        return http;
    }

    private static IDialogService DialogoQueConfirma()
    {
        var dialogo = Substitute.For<IDialogService>();
        var referencia = Substitute.For<IDialogReference>();
        referencia.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        dialogo.ShowAsync<Pages.Dialog.Dialog>(default, default, default)
               .ReturnsForAnyArgs(Task.FromResult(referencia));
        return dialogo;
    }

    [Fact]
    public async Task Resposta_401_com_token_marca_a_sessao_como_expirada()
    {
        var sessao = new SessionExpiration();
        var api = new ApiFalsa().Responde(HttpMethod.Get, CaminhoProduto, "", HttpStatusCode.Unauthorized);

        await Cliente(api, sessao).GetAsync(CaminhoProduto);

        Assert.True(sessao.IsExpired);
    }

    [Fact]
    public async Task Resposta_401_sem_token_nao_e_sessao_expirada()
    {
        var sessao = new SessionExpiration();
        var api = new ApiFalsa().Responde(HttpMethod.Get, CaminhoProduto, "", HttpStatusCode.Unauthorized);

        await Cliente(api, sessao, comToken: false).GetAsync(CaminhoProduto);

        Assert.False(sessao.IsExpired);
    }

    [Fact]
    public async Task Senha_errada_no_signin_nao_e_sessao_expirada()
    {
        var sessao = new SessionExpiration();
        var api = new ApiFalsa().Responde(HttpMethod.Post, "api/auth/v1/signin", "", HttpStatusCode.Unauthorized);

        await Cliente(api, sessao).PostAsync("api/auth/v1/signin", new StringContent("{}"));

        Assert.False(sessao.IsExpired);
    }

    [Fact]
    public async Task Resposta_de_sucesso_nao_mexe_na_sessao()
    {
        var sessao = new SessionExpiration();
        var api = new ApiFalsa().Responde(HttpMethod.Get, CaminhoProduto, new { id = 42 });

        await Cliente(api, sessao).GetAsync(CaminhoProduto);

        Assert.False(sessao.IsExpired);
    }

    [Fact]
    public void Token_vencido_mostra_so_o_aviso_de_sessao_e_faz_logout()
    {
        var sessao = new SessionExpiration();
        var dialogo = DialogoQueConfirma();
        var auth = Substitute.For<IAuthService>();

        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, "api/storage/employees/v1", Array.Empty<object>())
            .Responde(HttpMethod.Get, CaminhoProduto, "", HttpStatusCode.Unauthorized);

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(sessao);
        Services.AddSingleton(auth);
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(dialogo, sessao));
        Services.AddSingleton(new ApiExtensions(Cliente(api, sessao)));
        AddAuthorization().SetAuthorized("admin");

        Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();

            builder.OpenComponent<SessionExpiredWatcher>(1);
            builder.CloseComponent();

            builder.OpenComponent<EditProduct>(2);
            builder.AddAttribute(3, nameof(EditProduct.productId), 42);
            builder.CloseComponent();
        });

        var titulos = dialogo.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IDialogService.ShowAsync))
            .Select(c => (string)c.GetArguments()[0]!)
            .ToList();

        Assert.Equal(new[] { "Sessão expirada" }, titulos);
        auth.Received(1).Logout();
    }

    [Fact]
    public async Task Nova_tentativa_de_login_libera_os_dialogos()
    {
        var sessao = new SessionExpiration();
        await sessao.NotifyExpired();

        var api = new ApiFalsa().Responde(HttpMethod.Post, "api/auth/v1/signin", "", HttpStatusCode.Unauthorized);
        var http = new HttpClient(api) { BaseAddress = new Uri("http://localhost/") };
        var servico = new AuthService(http, new AuthStateProvider(Substitute.For<IJSRuntime>(), http), sessao);

        await Assert.ThrowsAsync<ApiException>(() => servico.Login(new UserVO { Username = "admin", Password = "errada" }));

        Assert.False(sessao.IsExpired);
    }
}
