using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartStorage.Configurations.Config.RabbitMQ
{
    public static class RabbitMQExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));

            services.AddSingleton(resolver => 
                resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMQSettings>>().Value);

            return services;
        }
    }
}
