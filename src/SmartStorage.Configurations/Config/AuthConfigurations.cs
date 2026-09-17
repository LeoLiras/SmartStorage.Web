using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartStorage.Shared.Auth;
using System.Text;

namespace SmartStorage.Configurations.Config
{
    public static class AuthConfigurations
    {
        public static IServiceCollection AddAuthConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var tokenConfigurations = new TokenConfiguration();

            configuration.GetSection("TokenConfigurations")
                .Bind(tokenConfigurations);

            if (string.IsNullOrWhiteSpace(tokenConfigurations.Secret))
                throw new InvalidOperationException(
                    "TokenConfigurations:Secret não configurado. Defina a chave na configuração " +
                    "ou pela variável de ambiente TokenConfigurations__Secret.");

            services.AddSingleton(tokenConfigurations);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme
                    = JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme
                    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = tokenConfigurations.Issuer,
                        ValidAudience = tokenConfigurations.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(tokenConfigurations.Secret))
                    };
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            return services;
        }
    }
}
