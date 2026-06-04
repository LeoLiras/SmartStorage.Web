using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SmartStorage.Blazor;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services;
using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage.Blazor.Utils.ShowDialog;
using SmartStorage.Blazor.Utils.Variables;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5100/") });

builder.Services.AddHttpClient<IReportsService, ReportsService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ReportsAPI"])
            ).AddHttpMessageHandler<AuthHandler>();

builder.Services.AddHttpClient<IEmailService, EmailService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:EmailAPI"])
            ).AddHttpMessageHandler<AuthHandler>();

builder.Services.AddHttpClient<IAiService, AiService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AIAPI"])
            ).AddHttpMessageHandler<AuthHandler>();

builder.Services.AddHttpClient<IAuthService, AuthService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AuthAPI"])
            ).AddHttpMessageHandler<AuthHandler>();

builder.Services.AddAuthorizationCore();

builder.Services.AddMudServices();

builder.Services.AddScoped<ApiExtensions>();
builder.Services.AddScoped<ShowDialog>();
builder.Services.AddScoped<VariablesExtensions>();

builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    provider => provider.GetRequiredService<AuthStateProvider>());

builder.Services.AddScoped<AuthHandler>();

await builder.Build().RunAsync();
