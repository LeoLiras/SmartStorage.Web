using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddPolicyConfig("Blazor", ["https://localhost:5000"]);

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("Blazor");

// Configure the HTTP request pipeline.

app.UseOcelot();

app.Run();
