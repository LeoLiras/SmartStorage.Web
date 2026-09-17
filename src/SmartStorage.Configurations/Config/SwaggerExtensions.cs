using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace SmartStorage.Shared.Config
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services, string apiName, string apiDescription, string apiVersion)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(apiVersion,
                    new OpenApiInfo
                    {
                        Title = apiName,
                        Version = apiVersion,
                        Description = apiDescription,
                        Contact = new OpenApiContact
                        {
                            Name = "Leonardo de Lira Siqueira",
                            Url = new Uri("https://github.com/LeoLiras")
                        }
                    });
            });

            return services;
        }
    }
}
