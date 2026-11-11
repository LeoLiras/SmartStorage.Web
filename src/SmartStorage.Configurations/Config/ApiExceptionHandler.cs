using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartStorage.Shared.Validation;

namespace SmartStorage.Configurations.Config
{
    public class ApiExceptionHandler : IExceptionHandler
    {
        public const string UnexpectedErrorMessage = "Erro inesperado. Tente novamente.";

        private readonly ILogger<ApiExceptionHandler> _logger;

        public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            string message;

            if (exception is BusinessException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
            }
            else
            {
                _logger.LogError(exception, "Erro inesperado em {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                message = UnexpectedErrorMessage;
            }

            // Texto puro, e não ProblemDetails, porque é o que o BadRequest(ex.Message) devolve hoje e
            // o que o ReadApiAsync do Blazor e o roteiro leem (D-16).
            httpContext.Response.ContentType = "text/plain; charset=utf-8";
            await httpContext.Response.WriteAsync(message, cancellationToken);

            return true;
        }
    }
}
