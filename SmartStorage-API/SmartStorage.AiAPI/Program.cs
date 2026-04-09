using SmartStorage.AIAPI.Utils;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwagger(Utils.apiName, Utils.apiDescription, Utils.apiVersion);

builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseApiDefaults(Utils.apiName, Utils.apiVersion);

app.UseAuthorization();

app.MapControllers();

app.Run();
