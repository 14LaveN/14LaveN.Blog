using Domain.Core.Utility;
using Identity.API.Persistence;
using Persistence;
using Persistence.Core.Extensions;

namespace IdentityApi.Common.DependencyInjection;

internal static class DiDatabase
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));

        services
            .AddBaseDatabase(configuration)
            .AddUserDatabase(configuration)
            .AddMigration<UserDbContext, UserDbContextSeed>();
        
        return services;
    }
}