using SmartStorage.AIAPI.Repository;
using SmartStorage.AIAPI.Repository.Interfaces;
using SmartStorage.AIAPI.Utils;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApiVersioning();

builder.Services.AddSwagger(Utils.apiName, Utils.apiDescription, Utils.apiVersion);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddPolicyConfig("Blazor", ["https://localhost:4480"]);

builder.Services.AddScoped<IAiRepository, AiRepository>();

var app = builder.Build();

app.UseApiDefaults(Utils.apiName, Utils.apiVersion, "Blazor");

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
