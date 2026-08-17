using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddPolicyConfig("Blazor", allowedOrigins);

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("Blazor");

app.UseHealthChecks("/health");

// Configure the HTTP request pipeline.

app.UseOcelot();

app.Run();
