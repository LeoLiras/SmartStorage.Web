using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.AuthenticationAPI.Contract.Tools;
using SmartStorage.AuthenticationAPI.Repositories;
using SmartStorage.AuthenticationAPI.Repositories.Implementations;
using SmartStorage.AuthenticationAPI.Services;
using SmartStorage.AuthenticationAPI.Services.Implementations;
using SmartStorage.AuthenticationAPI.Utils;
using SmartStorage.Configurations.Config;
using SmartStorage.Shared.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddApiVersioning();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();
builder.Services.AddScoped<IUserAuthService, UserAuthServiceImplementation>();
builder.Services.AddScoped<ILoginService, LoginServiceImplementation>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

builder.Services.AddPolicyConfig("Blazor", ["https://localhost:4480"]);

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddSwagger(Utils.apiName, Utils.apiDescription, Utils.apiVersion);

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseApiDefaults(Utils.apiName, Utils.apiVersion, "Blazor");

//app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
