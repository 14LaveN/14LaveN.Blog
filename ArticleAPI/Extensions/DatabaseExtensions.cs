using Application.Core.Extensions;
using ArticleAPI.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Persistence;

namespace ArticleAPI.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.TryAddScoped<IArticlesRepository, ArticlesRepository>();
        
        services
            .AddBaseDatabase(configuration);
        
        services
            .AddHealthChecks()
            .AddRedis(
                configuration.GetConnectionStringOrThrow("Redis"),
                name: "redis",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "redis" });
        
        return services;
    }
}