using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartStorage_API;
using SmartStorage_API.Hypermedia.Enricher;
using SmartStorage_API.Hypermedia.Filters;
using SmartStorage_API.Model.Context;
using SmartStorage_API.Service;
using SmartStorage_API.Service.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(Utils.apiVersion,
        new OpenApiInfo
        {
            Title = "SmartStorage - API",
            Version = Utils.apiVersion,
            Description = "SmartStorage - API",
            Contact = new OpenApiContact
            {
                Name = "Leonardo de Lira Siqueira",
                Url = new Uri("https://github.com/LeoLiras")
            }
        });
});

builder.Services.AddDbContext<SmartStorageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"), b => b.MigrationsAssembly("SmartStorage.Infrastructure")));

var filterOptions = new HyperMediaFilterOptions();
filterOptions.ContentResponseEnricherList.Add(new EmployeeEnricher());
filterOptions.ContentResponseEnricherList.Add(new ProductEnricher());
filterOptions.ContentResponseEnricherList.Add(new SaleEnricher());
filterOptions.ContentResponseEnricherList.Add(new ShelfEnricher());
filterOptions.ContentResponseEnricherList.Add(new EnterEnricher());

builder.Services.AddSingleton(filterOptions);

builder.Services.AddApiVersioning();

builder.Services.AddScoped<IEmployeeBusiness, EmployeeBusinessImplementation>();
builder.Services.AddScoped<IProductBusiness, ProductBusinessImplementation>();
builder.Services.AddScoped<ISaleBusiness, SaleBusinessImplementation>();
builder.Services.AddScoped<IShelfBusiness, ShelfBusinessImplementation>();

builder.Configuration.AddEnvironmentVariables();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Blazor",
        policy => policy
            .WithOrigins(allowedOrigins ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("Blazor");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/swagger/{Utils.apiVersion}/swagger.json",
        $"SmartStorage - API - {Utils.apiVersion}");
});

var options = new RewriteOptions();
options.AddRedirect("^$", "swagger");

app.UseRewriter(options);

app.MapControllers();

app.MapControllerRoute("DefaultApi", "{controller=values}/v{version=apiVersion}/{id?}");

app.Run();
