using System.Net;
using System.Reflection;
using Application;
using Application.ApiHelpers.Middlewares;
using EmailService;
using Identity.Api.Common.DependencyInjection;
using Identity.API.Common.DependencyInjection;
using Identity.API.Infrastructure;
using IdentityApi.Common.DependencyInjection;
using IdentityApi.IntegrationEvents.User.Events.UserCreated;
using IdentityApi.IntegrationEvents.User.Handlers.UserCreated;
using Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMq;
using RabbitMq.Extensions;
using ServiceDefaults;

namespace Identity.API.Extensions;

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
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;

            });
            
            options.Listen(IPAddress.Any, 6556, listenOptions =>
            {
                listenOptions.UseHttps();
            });
    
            options.Limits.MaxRequestBodySize = 10 * 1024; // 10 KB
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            options.Limits.MaxConcurrentConnections = 100;
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
        });   
        
        builder
            .AddEndpoints(typeof(DiMediator).Assembly)
            .AddAuthorizationExtension(builder.Configuration)
            .AddDatabase(builder.Configuration)
            .AddMediatr();
        
        builder
            .AddCachingDefaults("Identity_")
            .AddEmailService(builder.Configuration)
            .AddInfrastructure()
            .AddApplication()
            .AddIdentityInfrastructure()
            .AddDatabase(builder.Configuration) 
            .AddSwagger(Assembly.GetExecutingAssembly(), 1, 0);
        
        builder.Services
            .AddRabbitMq(builder.Configuration)
            .AddSubscription<UserCreatedIntegrationEvent, SendWelcomeEmailOnUserCreatedIntegrationEventHandler>()
            .ConfigureJsonOptions(options => options.TypeInfoResolverChain.Add(IntegrationEventContext.Default));

        builder.Services.TryAddTransient<LogContextEnrichmentMiddleware>();
    }
}