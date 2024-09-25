using Application.ApiHelpers.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Application.ApiHelpers.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddlewares(
        this IApplicationBuilder app,
        ILogger logger)

    {        
        ArgumentNullException.ThrowIfNull(app);

        app
            .UseMiddleware<IdempotentRequestMiddleware>()
            .UseMiddleware<RequestLoggingMiddleware>(logger)
            //TODO .UseMiddleware<ResponseCachingMiddleware>()
            .UseMiddleware<LogContextEnrichmentMiddleware>();

        return app;
    }
}