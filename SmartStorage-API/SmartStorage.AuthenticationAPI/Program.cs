using SmartStorage.AuthenticationAPI.Config;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.AuthenticationAPI.Contract.Tools;
using SmartStorage.AuthenticationAPI.Repositories;
using SmartStorage.AuthenticationAPI.Repositories.Implementations;
using SmartStorage.AuthenticationAPI.Services;
using SmartStorage.AuthenticationAPI.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthConfiguration(builder.Configuration);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();
builder.Services.AddScoped<IUserAuthService, UserAuthServiceImplementation>();
builder.Services.AddScoped<ILoginService, LoginServiceImplementation>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
