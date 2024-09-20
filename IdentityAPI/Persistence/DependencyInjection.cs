using Application.Core.Abstractions;
using Application.Core.Extensions;
using Identity.API.Domain.Repositories;
using Identity.API.Persistence.Repositories;
using IdentityApi.Persistence.Repositories;
using MediatR.NotificationPublishers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Infrastructure;

namespace Identity.API.Persistence;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddUserDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        ConnectionString connectionString = configuration.GetConnectionStringOrThrow(ConnectionString.SettingsKey);

        services.TryAddSingleton<ConnectionString>(_ => connectionString);
        
        //TODO services
        //TODO     .AddHealthChecks()
        //TODO     .AddCheck<DbContextHealthCheck<UserDbContext>>(
        //TODO         "UserDatabase",
        //TODO         failureStatus: HealthStatus.Unhealthy,
        //TODO         tags: new[] { "db", "sql" });
        
        services.AddDbContext<UserDbContext>((sp, o) =>
            o.UseNpgsql(connectionString, act
                    =>
                {
                    act.EnableRetryOnFailure(3);
                    act.CommandTimeout(30);
                    act.MigrationsAssembly("IdentityApi");
                    act.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables))
                .LogTo(Console.WriteLine)
                .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
                .EnableServiceProviderCaching()
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

        services
            .AddScoped<IUserUnitOfWork, UserUnitOfWork>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IDbContext, UserDbContext>();
        
        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<IdentityApi.Program>();

            x.NotificationPublisher = new TaskWhenAllPublisher();
            x.NotificationPublisherType = typeof(TaskWhenAllPublisher);
        });
        
        return services;
    }
}