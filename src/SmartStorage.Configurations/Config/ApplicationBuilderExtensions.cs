using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;

namespace SmartStorage.Shared.Config
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiDefaults(this IApplicationBuilder app, string apiName, string apiVersion, string apiCors = "")
        {
            // Primeiro do pipeline, para pegar também o que falhar na autenticação; quem responde é o
            // ApiExceptionHandler, que cada Program registra com AddExceptionHandler (D-15). O pipeline
            // vazio só existe porque o UseExceptionHandler() sem argumento derruba o startup quando não há
            // ProblemDetails; ele nunca roda, porque o ApiExceptionHandler trata toda exceção.
            app.UseExceptionHandler(_ => { });

            app.UseHealthChecks("/health");

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
