using Microsoft.EntityFrameworkCore;
using SmartStorage_API.Hypermedia.Enricher;
using SmartStorage_API.Hypermedia.Filters;
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

var filterOptions = new HyperMediaFilterOptions();
filterOptions.ContentResponseEnricherList.Add(new EmployeeEnricher());
filterOptions.ContentResponseEnricherList.Add(new ProductEnricher());
filterOptions.ContentResponseEnricherList.Add(new SaleEnricher());

builder.Services.AddSingleton(filterOptions);

builder.Services.AddApiVersioning();

builder.Services.AddScoped<IEmployeeBusiness, EmployeeBusinessImplementation>();
builder.Services.AddScoped<IProductBusiness, ProductBusinessImplementation>();
builder.Services.AddScoped<ISaleBusiness, SaleBusinessImplementation>();
builder.Services.AddScoped<IShelfBusiness, ShelfBusinessImplementation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapControllerRoute("DefaultApi", "{controller=values}/{id?}");

app.Run();
