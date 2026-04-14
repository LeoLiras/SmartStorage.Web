using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Config;
using SmartStorage_API;
using SmartStorage_API.Authentication.Config;
using SmartStorage_API.Authentication.Contract;
using SmartStorage_API.Authentication.Contract.Tools;
using SmartStorage_API.Authentication.Repositories;
using SmartStorage_API.Authentication.Repositories.Implementations;
using SmartStorage_API.Authentication.Services;
using SmartStorage_API.Authentication.Services.Implementations;
using SmartStorage_API.Hypermedia.Enricher;
using SmartStorage_API.Hypermedia.Filters;
using SmartStorage_API.Model.Context;
using SmartStorage_API.Service;
using SmartStorage_API.Service.Implementations;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers();

builder.Services.AddAuthConfiguration(builder.Configuration);

var filterOptions = new HyperMediaFilterOptions();
filterOptions.ContentResponseEnricherList.Add(new EmployeeEnricher());
filterOptions.ContentResponseEnricherList.Add(new ProductEnricher());
filterOptions.ContentResponseEnricherList.Add(new SaleEnricher());
filterOptions.ContentResponseEnricherList.Add(new ShelfEnricher());
filterOptions.ContentResponseEnricherList.Add(new EnterEnricher());

builder.Services.AddSingleton(filterOptions);

builder.Services.AddApiVersioning();

builder.Services.AddScoped<IEmployeeBusiness, EmployeeBusinessImplementation>();
builder.Services.AddScoped<IProductBusiness, ProductBusinessImplementation>();
builder.Services.AddScoped<ISaleBusiness, SaleBusinessImplementation>();
builder.Services.AddScoped<IShelfBusiness, ShelfBusinessImplementation>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();
builder.Services.AddScoped<IUserAuthService, UserAuthServiceImplementation>();
builder.Services.AddScoped<ILoginService, LoginServiceImplementation>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSwagger(Utils.apiName, Utils.apiDescription, Utils.apiVersion);
builder.Services.AddDatabase(builder.Configuration);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddPolicyConfig("Blazor", allowedOrigins);

var app = builder.Build();

app.UseApiDefaults(Utils.apiName, Utils.apiVersion, "Blazor");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute("DefaultApi", "{controller=values}/v{version=apiVersion}/{id?}");

app.Run();
