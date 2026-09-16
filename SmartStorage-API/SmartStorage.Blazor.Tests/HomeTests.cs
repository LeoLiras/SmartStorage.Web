using System.Net.Http;
using System.Security.Claims;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Pages;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.Variables;
using SmartStorage_Shared.Model;
using Dialogo = SmartStorage.Blazor.Utils.ShowDialog.ShowDialog;

namespace SmartStorage.Blazor.Tests;

public class HomeTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private IRenderedComponent<Home> Renderiza(string papel)
    {
        var vazio = Array.Empty<object>();
        var api = new ApiFalsa()
            .Responde(HttpMethod.Get, "api/storage/products/v1", vazio)
            .Responde(HttpMethod.Get, "api/storage/employees/v1", vazio)
            .Responde(HttpMethod.Get, "api/storage/shelf/v1", vazio)
            .Responde(HttpMethod.Get, "api/storage/shelf/v1/allocation", vazio)
            .Responde(HttpMethod.Get, "api/storage/sales/v1", vazio);

        var auth = Substitute.For<IAuthService>();
        auth.GetUser("admin").Returns(Task.FromResult(new User { Id = 1, Username = "admin", FullName = "Ana Souza" }));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(auth);
        Services.AddSingleton(new VariablesExtensions());
        Services.AddSingleton(new Dialogo(Substitute.For<IDialogService>(), new SessionExpiration()));
        Services.AddSingleton(new ApiExtensions(new HttpClient(api) { BaseAddress = new Uri("http://localhost/") }));
        Services.AddSingleton<IProductService>(new ProductService(api.Cliente()));
        Services.AddSingleton<IShelfService>(new ShelfService(api.Cliente()));
        Services.AddSingleton<ISaleService>(new SaleService(api.Cliente()));
        AddAuthorization().SetAuthorized("admin").SetRoles(papel).SetClaims(new Claim("unique_name", "admin"));

        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.Contains("Bem-vindo", cut.Markup));

        return cut;
    }

    [Fact]
    public void Boas_vindas_mostram_o_nome_e_o_perfil()
    {
        var cut = Renderiza("Administrador");

        cut.WaitForAssertion(() => Assert.Contains("Bem-vindo, Ana Souza", cut.Find("h4").TextContent));
        Assert.Equal("Administrador", cut.Find(".app-welcome .app-chip").TextContent.Trim());
    }

    [Fact]
    public void Atalhos_levam_as_telas_do_menu()
    {
        var cut = Renderiza("Usuario");

        var destinos = cut.FindAll(".app-shortcut").Select(a => a.GetAttribute("href")).ToList();

        Assert.Equal(new[] { "products", "products/shelves", "products/sales", "products/insight", "user/account" }, destinos);
        Assert.Equal("Usuário", cut.Find(".app-welcome .app-chip").TextContent.Trim());
    }
}
