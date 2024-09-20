using Identity.API.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Domain.Core.Utility;
using Identity.API.Domain.Entities;
using IdentityApi.Infrastructure.Authentication.SignIn;

namespace Identity.API.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));
        
        services.AddScoped<SignInProvider<User>>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        
        return services;
    }
}