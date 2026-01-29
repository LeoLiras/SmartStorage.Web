using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SmartStorage.Blazor;
using SmartStorage.Blazor.Auth;
using SmartStorage.Blazor.Utils;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5208/") });

builder.Services.AddAuthorizationCore();

builder.Services.AddMudServices();

builder.Services.AddScoped<ApiExtensions>();
builder.Services.AddScoped<ShowDialog>();
builder.Services.AddScoped<VariablesExtensions>();

builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthStateProvider>(provider => provider.GetRequiredService<AuthStateProvider>());
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>(provider => provider.GetRequiredService<AuthStateProvider>());

await builder.Build().RunAsync();
