using SmartStorage.AIAPI.Repository;
using SmartStorage.AIAPI.Repository.Interfaces;
using SmartStorage.AIAPI.Utils;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwagger(Utils.apiName, Utils.apiDescription, Utils.apiVersion);

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddApiVersioning();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Blazor", policy =>
    {
        policy
            .WithOrigins("http://localhost:5001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IAiRepository, AiRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("Blazor");

app.UseApiDefaults(Utils.apiName, Utils.apiVersion);

app.UseAuthorization();

app.MapControllers();

app.Run();
