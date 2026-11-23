using System.Net.Http;
using System.Text;
using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services;
using SmartStorage.Shared.VO;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// O login devolve o access token e o refresh token, mas o Blazor guardava só o primeiro:
/// sem o refresh token no navegador, a sessão não tem como ser renovada quando o access
/// token de 60 minutos vence.
/// </summary>
public class GuardaDoRefreshTokenTests : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private static string Jwt()
    {
        static string Base64Url(string json)
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var payload = JsonSerializer.Serialize(new Dictionary<string, object> { ["unique_name"] = "admin" });

        return $"{Base64Url("{\"alg\":\"HS256\",\"typ\":\"JWT\"}")}.{Base64Url(payload)}.assinatura";
    }

    [Fact]
    public async Task Login_guarda_o_access_token_e_o_refresh_token()
    {
        var accessToken = Jwt();
        var api = new ApiFalsa().Responde(HttpMethod.Post, "api/auth/v1/signin",
            new { authenticated = true, accessToken, refreshToken = "refresh-do-admin" });

        JSInterop.Mode = JSRuntimeMode.Loose;
        var js = Services.GetRequiredService<IJSRuntime>();
        var servico = new AuthService(api.Cliente(), new AuthStateProvider(js), new SessionExpiration());

        await servico.Login(new UserVO { Username = "admin", Password = "admin123" });

        var gravados = JSInterop.Invocations["localStorage.setItem"]
            .ToDictionary(i => (string)i.Arguments[0]!, i => (string)i.Arguments[1]!);

        Assert.Equal(accessToken, gravados[AuthStateProvider.tokenKey]);
        Assert.Equal("refresh-do-admin", gravados[AuthStateProvider.refreshTokenKey]);
    }
}
