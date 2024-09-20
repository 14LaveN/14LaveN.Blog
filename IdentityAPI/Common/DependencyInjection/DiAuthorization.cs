using System.Text;
using System.Text.Json;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Domain.Core.Utility;
using Identity.API.Domain.Entities;
using Identity.API.Infrastructure.Settings.User;
using Identity.API.Persistence;
using IdentityApi.Infrastructure.Settings.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Identity.Api.Common.DependencyInjection;

internal static class DiAuthorization
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddAuthorizationExtension(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));
        
        services.TryAddScoped<UserManager<User>>();
        
        services.AddHttpContextAccessor();
        
        //TODO services.AddAuth0WebAppAuthentication(options =>
        //TODO {
        //TODO     options.Domain = configuration["Auth0:Domain"];
        //TODO     options.ClientId = configuration["Auth0:ClientId"];
        //TODO });

        
        services.AddIdentity<User, IdentityRole<Ulid>>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();
        
        services
            .AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = "303301197621-a1hqp3v3kte5jh27go4pjol1s3vcjh30.apps.googleusercontent.com";
                googleOptions.ClientSecret = "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";
            })
            .AddOpenIdConnect("Auth0", options =>
                {
                    options.Authority = $"https://{configuration["Auth0:Domain"]}";
                    options.ClientId = configuration["Auth0:ClientId"];
                    options.ClientSecret = configuration["Auth0:ClientSecret"];
                    options.ResponseType = OpenIdConnectResponseType.Code;

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidIssuer = "https://dev-l8s5s4hhicih7rqg.us.auth0.com/",
                        ValidAudience = "https://6556",
                        IssuerSigningKey = new SymmetricSecurityKey("PartMm7wc3o7oIeDJUhlfsf67VeJhnx3"u8.ToArray())
                    };

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    options.CallbackPath = new PathString("/callback");

                    options.ClaimsIssuer = "Auth0";
                    
                    options.SaveTokens = true;

                    // Обработка событий аутентификации
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProviderForSignOut = context =>
                        {
                            var logoutUri = $"https://{configuration["Auth0:Domain"]}/v2/logout?client_id={configuration["Auth0:ClientId"]}";

                            var postLogoutUri = context.Properties.RedirectUri;
                            if (!string.IsNullOrEmpty(postLogoutUri))
                            {
                                if (postLogoutUri.StartsWith("/"))
                                {
                                    var request = context.Request;
                                    postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                                }
                                logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                            }

                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        },
                        
                        OnTokenValidated = async context =>
                        {
                            // Логика обработки после валидации токена
                            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)context.Principal.Identity;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            context.Response.Redirect("/Error?message=" + context.Exception.Message);
                            context.HandleResponse(); // Остановить дальнейшую обработку
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCookie()
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.Authority = "https://localhost:5001";  
                options.Audience = "identityApi"; 
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "https://dev-l8s5s4hhicih7rqg.us.auth0.com/",
                    ValidAudience = "https://localhost:6556",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("NMEQEvJzylfDMBaS3fsgG48g4LDBdGnu"))
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                
                        if (string.IsNullOrEmpty(context.Error))
                            context.Error = "invalid_token";
                        
                        if (string.IsNullOrEmpty(context.ErrorDescription))
                            context.ErrorDescription = "This request requires a valid JWT access token to be provided";
                
                        if (context.AuthenticateFailure == null ||
                            context.AuthenticateFailure.GetType() != typeof(SecurityTokenExpiredException))
                            return context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                error = context.Error,
                                error_description = context.ErrorDescription
                            }));
                        var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                        context.Response.Headers.Append("x-token-expired", authenticationException?.Expires.ToString("o"));
                        context.ErrorDescription =
                            $"The token expired on {authenticationException?.Expires:o}";
                
                        return context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            error = context.Error,
                            error_description = context.ErrorDescription
                        }));
                    }
                };
            });
        
        services.AddIdentityServer()
            .AddAspNetIdentity<User>()
            .AddInMemoryApiResources(IdentityConfiguration.ApiResources)
            .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
            .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
            .AddInMemoryClients(IdentityConfiguration.Clients)
            .AddDeveloperSigningCredential();
        
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SettingsKey)
            .ValidateOnStart();
        
        services.AddCors(options => options.AddDefaultPolicy(corsPolicyBuilder =>
            corsPolicyBuilder.WithOrigins("https://localhost:44442", "http://localhost:44460")
                .AllowAnyHeader()
                .AllowAnyMethod()));
        
        return services;
    }
}