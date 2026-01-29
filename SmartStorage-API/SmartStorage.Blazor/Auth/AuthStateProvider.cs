using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace SmartStorage.Blazor.Auth
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await Task.Delay(4000);
            
            var usuario = new ClaimsIdentity("demo");

            return await Task.FromResult(new AuthenticationState(
                new ClaimsPrincipal(usuario)));

        }
    }
}
