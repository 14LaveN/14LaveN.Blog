using System.Reflection;
using Application.ApiHelpers.Configurations;
using Domain.Core.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ServiceDefaults;

public static class SwaggerExtensions
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly.</param>
    /// <param name="majorVersion">The major version.</param>
    /// <param name="minorVersion">The minor version.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddSwagger(
        this IServiceCollection services, 
        Assembly assembly,
        int majorVersion,
        int minorVersion)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));
        
        services.AddSwachbackleService(
            assembly,
            assembly.GetName().Name!);
        
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });
        
        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        
        return services;
    }
    
    public static IServiceCollection AddSwachbackleService(
        this IServiceCollection services,
        Assembly assembly,
        string apiName)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));

        return services.AddSwaggerGen(options =>
        {
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Version = description.ApiVersion.ToString(),
                    Title = apiName,
                    Description = $"Backend Web API на C# .NET for {apiName} application {description.GroupName}",
                    Contact = new OpenApiContact
                    {
                        Name = "GitHub",
                        Url = new Uri("https://github.com/14LaveN")
                    }
                });
            }

            if (apiName is not "Identity.Api")
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header используется в расшифровке запроса.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            }

            var xmlFilename = $"{assembly.GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
    }

    public static IApplicationBuilder UseSwaggerApp(this IApplicationBuilder app)
    {
        if (app is null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();  
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
            options.RoutePrefix = string.Empty;
        });
        return app;
    }
    
}