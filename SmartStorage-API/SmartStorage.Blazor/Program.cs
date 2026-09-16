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
using SmartStorage.Blazor.Utils.Cart;
using SmartStorage.Blazor.Utils.ShowDialog;
using SmartStorage.Blazor.Utils.Variables;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<SessionExpiration>();

builder.Services.AddScoped(sp => new HttpClient(new SessionExpiredHandler(sp.GetRequiredService<SessionExpiration>())
{
    InnerHandler = new HttpClientHandler()
})
{
    BaseAddress = new Uri(builder.Configuration["ServiceUrls:SmartStorageAPI"])
});

builder.Services.AddHttpClient<IReportsService, ReportsService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ReportsAPI"])
            ).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<SessionExpiredHandler>();

builder.Services.AddHttpClient<IEmailService, EmailService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:EmailAPI"])
            ).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<SessionExpiredHandler>();

builder.Services.AddHttpClient<IAiService, AiService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AIAPI"])
            ).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<SessionExpiredHandler>();

builder.Services.AddHttpClient<ISaleService, SaleService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:SmartStorageAPI"])
            ).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<SessionExpiredHandler>();

builder.Services.AddHttpClient<IShelfService, ShelfService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:SmartStorageAPI"])
            ).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<SessionExpiredHandler>();

builder.Services.AddHttpClient<IProductService, ProductService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:SmartStorageAPI"])
            ).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<SessionExpiredHandler>();

builder.Services.AddHttpClient<IEmployeeService, EmployeeService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:SmartStorageAPI"])
            ).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<SessionExpiredHandler>();

builder.Services.AddHttpClient<IAuthService, AuthService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AuthAPI"])
            ).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<SessionExpiredHandler>();

builder.Services.AddAuthorizationCore();

builder.Services.AddMudServices();

builder.Services.AddScoped<ApiExtensions>();
builder.Services.AddScoped<ShowDialog>();
builder.Services.AddScoped<VariablesExtensions>();
builder.Services.AddScoped<SaleCart>();

builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    provider => provider.GetRequiredService<AuthStateProvider>());

builder.Services.AddScoped<AuthHandler>();
builder.Services.AddTransient<SessionExpiredHandler>();

await builder.Build().RunAsync();
