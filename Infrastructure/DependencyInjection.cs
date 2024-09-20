using Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Common;
using Application.Core.Abstractions.Common;
using Application.Core.Abstractions.Helpers.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddHttpContextAccessor();
        
        services
            .AddScoped<IDateTime, MachineDateTime>()
            .AddScoped<IUserIdentifierProvider, UserIdentifierProvider>()
            .AddScoped<IPermissionProvider, PermissionProvider>()
            .AddScoped<IUserNameProvider, UserNameProvider>();
            
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
        return services;
    }
}