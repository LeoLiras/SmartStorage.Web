using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using SmartStorage.Configurations.Config;
using SmartStorage.ReportsAPI.Repository;
using SmartStorage.ReportsAPI.Repository.IRepository;
using SmartStorage.ReportsAPI.Utils;
using SmartStorage.Shared.Config;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddSharedConfiguration();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddApiVersioning();

builder.Services.AddSwagger(Utils.apiName, Utils.apiDescription, Utils.apiVersion);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddPolicyConfig("Blazor", ["https://localhost:4480"]);
builder.Services.AddAuthConfiguration(builder.Configuration);

builder.Services.AddScoped<IReportRepository, ReportRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseApiDefaults(Utils.apiName, Utils.apiVersion, "Blazor");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
