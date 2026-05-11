using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddPolicyConfig("Blazor", ["https://localhost:4480"]);

builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:5200/";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "smart_storage");
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseApiDefaults(Utils.apiName, Utils.apiVersion, "Blazor");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
