using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SmartStorage.Blazor.Utils.Local_Storage;
using System.Security.Claims;
using System.Text.Json;

namespace SmartStorage.Blazor.Provider
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        public static readonly string tokenKey = "tokenKey";

        private AuthenticationState notAuthenticate =>
           new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        private readonly IJSRuntime js;

        public AuthStateProvider(IJSRuntime js)
        {
            this.js = js;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await js.GetFromLocalStorage(tokenKey);

            if (string.IsNullOrEmpty(token))
            {
                return notAuthenticate;
            }
            return CreateAuthenticationState(token);
        }

        public AuthenticationState CreateAuthenticationState(string token)
        {
            //extrair as claims
            return new AuthenticationState(new ClaimsPrincipal
                (new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer
                .Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (roles != null)
            {
                if (roles.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                    foreach (var parsedRole in parsedRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }
                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp =>
            new Claim(kvp.Key, kvp.Value.ToString())));
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        public async Task Logout()
        {
            try
            {
                await js.RemoveItem(tokenKey);
                NotifyAuthenticationStateChanged(Task.FromResult(notAuthenticate));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task Login(string accessToken)
        {
            try
            {
                await js.SetInLocalStorage(tokenKey, accessToken);
                var authState = CreateAuthenticationState(accessToken);
                NotifyAuthenticationStateChanged(Task.FromResult(authState));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
