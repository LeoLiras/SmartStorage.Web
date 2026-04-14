using Microsoft.Extensions.DependencyInjection;

namespace SmartStorage.Configurations.Config
{
    public static class PolicyExtensions
    {
        public static IServiceCollection AddPolicyConfig(this IServiceCollection services, string corsName, string[] allowedOrigins)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(corsName, policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins ?? Array.Empty<string>())
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
