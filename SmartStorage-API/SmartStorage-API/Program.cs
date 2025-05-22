using Microsoft.EntityFrameworkCore;
using SmartStorage_API.Model.Context;
using SmartStorage_API.Service;
using SmartStorage_API.Service.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddDbContext<SmartStorageContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection:ConnectionString")));

builder.Services.AddScoped<IEmployeeService, EmployeeServiceImplementation>();
builder.Services.AddScoped<IProductService, ProductServiceImplementation>();
builder.Services.AddScoped<ISaleService, SaleServiceImplementation>();
builder.Services.AddScoped<IShelfService, ShelfServiceImplementation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
