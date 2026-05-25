using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SmartStorage.Blazor;
using SmartStorage.Blazor.Auth;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.ShowDialog;
using SmartStorage.Blazor.Utils.Variables;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5101/") });

builder.Services.AddHttpClient<IReportsService, ReportsService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ReportsAPI"])
            );

builder.Services.AddHttpClient<IEmailService, EmailService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:EmailAPI"])
            );

builder.Services.AddHttpClient<IAiService, AiService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AIAPI"])
            );

builder.Services.AddAuthorizationCore();

builder.Services.AddMudServices();

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority =
        builder.Configuration["ServiceUrls:IdentityServer"];

    options.ProviderOptions.ClientId = "smart_storage";

    options.ProviderOptions.ResponseType = "code";

    options.ProviderOptions.DefaultScopes.Add("smart_storage");

    options.UserOptions.RoleClaim = "role";
    options.UserOptions.NameClaim = "name";
});

builder.Services.AddScoped<ApiExtensions>();
builder.Services.AddScoped<ShowDialog>();
builder.Services.AddScoped<VariablesExtensions>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAuthService, AuthService>(provider => provider.GetRequiredService<AuthService>());
builder.Services.AddScoped<AuthenticationStateProvider, AuthService>(provider => provider.GetRequiredService<AuthService>());

await builder.Build().RunAsync();
