using Application.Core.Abstractions.Idempotency;
using Application.Core.Abstractions.Redis;
using Application.Core.Settings;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Persistence.Core.Abstractions.HealthChecks;
using Persistence.Data.Repositories;
using Persistence.Idempotency;
using Persistence.Infrastructure;
using Persistence.Redis;

namespace Persistence;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddBaseDatabase(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services
            .AddMongo(configuration)
            .AddBase(configuration);
        
        return services;
    }

    public static IServiceCollection AddBase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        var connectionString = configuration.GetConnectionString(ConnectionString.SettingsKey);
        
        services.AddDbContext<BaseDbContext>((sp, o) =>
            o.UseNpgsql(connectionString, act
                    =>
                {
                    act.EnableRetryOnFailure(3);
                    act.CommandTimeout(30);
                    act.MigrationsAssembly("Persistence");
                    act.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables))
                .LogTo(Console.WriteLine)
                .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
                .EnableServiceProviderCaching()
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.TryAddKeyedScoped<IDbContext, BaseDbContext>(typeof(BaseDbContext));
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.TryAddScoped<IRedisPubSubService, RedisPubSubService>();
        
        //TODO services
        //TODO     .AddHealthChecks()
        //TODO     .AddCheck<DbContextHealthCheck<BaseDbContext>>(
        //TODO         "BaseDatabase",
        //TODO         failureStatus: HealthStatus.Unhealthy,
        //TODO         tags: new[] { "db", "sql" });

        return services;
    }
    
    public static IServiceCollection AddMongo(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.MongoSettingsKey));
        
        services
            .AddOptions<MongoSettings>()
            .BindConfiguration(MongoSettings.MongoSettingsKey)
            .ValidateOnStart();

        //TODO services
        //TODO     .AddHealthChecks()
        //TODO     .AddCheck<MongoDbHealthCheck>("mongodb", 
        //TODO         failureStatus: HealthStatus.Unhealthy, 
        //TODO         tags: new[] { "db", "mongodb" });
        
        services.AddSingleton<IMetricsRepository, MetricsRepository>()
            .AddSingleton<IMongoRepository<RabbitMessage>, RabbitMessagesRepository>()
            .AddSingleton<ICommonMongoDbContext, CommonMongoDbContext>();
        
        services.TryAddTransient<MongoSettings>();

        return services;
    }
}