using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;

namespace SmartStorage.Shared.Config
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiDefaults(this IApplicationBuilder app, string apiName, string apiVersion, string apiCors = "")
        {
            app.UseRouting();

            if (!string.IsNullOrWhiteSpace(apiCors))
                app.UseCors(apiCors);

            app.UseAuthentication();
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json",
                    $"{apiName} - {apiVersion}");
            });

            var options = new RewriteOptions();
            options.AddRedirect("^$", "swagger");

            app.UseRewriter(options);

            return app;
        }
    }
}
