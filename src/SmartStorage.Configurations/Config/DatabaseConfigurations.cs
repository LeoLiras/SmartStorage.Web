using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartStorage_API.Model.Context;

namespace SmartStorage.Configurations.Config
{
    public static class DatabaseConfigurations
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SmartStorageContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"), b => b.MigrationsAssembly(typeof(SmartStorageContext).Assembly.GetName().Name)));

            return services;
        }
    }
}
