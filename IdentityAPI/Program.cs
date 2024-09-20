using System.Text.Json.Serialization;
using Application.ApiHelpers.Extensions;
using AspNetCore.Serilog.RequestLoggingMiddleware;
using Common.Logging;
using HealthChecks.UI.Client;
using Identity.API.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;
using Serilog;
using ServiceDefaults;
using IdentityApi.Components;
using IdentityApi.IntegrationEvents.User.Events.UserCreated;

#region BuilderRegion

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddServiceDefaults();

builder.AddApplicationServices(builder.WebHost);

builder.Host.UseSerilog(Logging.ConfigureLogger);

#endregion

#region ApplicationRegion

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwaggerApp();

app.UseRateLimiter();

app.UseCors();

app.UseHttpsRedirection();

app.UseRouting();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseIdentityServer();

MapEndpoints();

app.UseMetricsExtension();

app.UseSerilogRequestLogging();

app.UseCustomMiddlewares(app.Logger);

app.Run();

#endregion

void MapEndpoints()
{
    ArgumentNullException.ThrowIfNull(app);
    
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.MapControllers();

    app.MapMetrics();
    
    app
        .MapDefaultEndpoints()
        .MapEndpoints();
}

[JsonSerializable(typeof(UserCreatedIntegrationEvent))]
partial class IntegrationEventContext : JsonSerializerContext;