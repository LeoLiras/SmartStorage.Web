using QuestPDF.Infrastructure;
using SmartStorage.Configurations.Config;
using SmartStorage.ReportsAPI.Repository;
using SmartStorage.ReportsAPI.Repository.IRepository;
using SmartStorage.ReportsAPI.Utils;
using SmartStorage.Shared.Config;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApiVersioning();

builder.Services.AddSwagger(Utils.apiName, Utils.apiDescription, Utils.apiVersion);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddPolicyConfig("Blazor", ["http://localhost:5001"]);

builder.Services.AddScoped<IReportRepository, ReportRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseApiDefaults(Utils.apiName, Utils.apiVersion, "Blazor");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
