using System.Net;
using System.Reflection;
using Application;
using Application.ApiHelpers.Middlewares;
using EmailService;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence;
using ServiceDefaults;

namespace ArticleAPI.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(
        this IHostApplicationBuilder builder,
        IWebHostBuilder webHost)
    {
        webHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Any, 6000, listenOptions =>
            {
                listenOptions.UseConnectionLogging();
        
            });
            options.ListenLocalhost(6001, listenOptions =>
            {
                listenOptions.UseHttps();
            });
    
            options.Limits.MaxRequestBodySize = 10 * 1024; // 10 KB
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            options.Limits.MaxConcurrentConnections = 100;
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
        });
        
        builder.AddDefaultAuthentication();
        
        builder
            .AddEndpoints(typeof(Program).Assembly)
            .AddDatabase(builder.Configuration)
            .AddMediatr();
        
        builder
            .AddCachingDefaults("Identity_")
            .AddEmailService(builder.Configuration)
            .AddInfrastructure()
            .AddApplication()
            .AddSwagger(Assembly.GetExecutingAssembly(), 1, 0);
        
        builder.Services.TryAddTransient<LogContextEnrichmentMiddleware>();
    }
}