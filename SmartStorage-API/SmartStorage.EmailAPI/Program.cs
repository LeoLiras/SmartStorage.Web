using SmartStorage.Configurations.Config;
using SmartStorage.EmailAPI.Utils;
using SmartStorage.Shared.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApiVersioning();

builder.Services.AddSwagger(Utils.apiName, Utils.apiDescription, Utils.apiVersion);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddPolicyConfig("Blazor", ["http://localhost:5001"]);

var app = builder.Build();

app.UseApiDefaults(Utils.apiName, Utils.apiVersion, "Blazor");

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
