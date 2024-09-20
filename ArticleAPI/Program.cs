using System.Text.Json.Serialization;
using Application.ApiHelpers.Extensions;
using ArticleAPI.Extensions;
using ArticleAPI.Services;
using AspNetCore.Serilog.RequestLoggingMiddleware;
using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Prometheus;
using Serilog;
using ServiceDefaults;

#region BuilderRegion

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices(builder.WebHost);  
builder.AddServiceDefaults();
builder.Host.UseSerilog(Logging.ConfigureLogger);

builder.Services.AddGrpc();

#endregion

#region ApplicationRegion

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwaggerApp();

app.UseRateLimiter();

app.UseCors();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

MapEndpoints();  

app.UseMetricsExtension();

app.UseSerilogRequestLogging();

app.UseCustomMiddlewares(app.Logger);

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();

await app.RunAsync();

void MapEndpoints()
{
    ArgumentNullException.ThrowIfNull(app);
    
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapMetrics();
    
    app
        .MapDefaultEndpoints()
        .MapEndpoints();
}

#endregion

[JsonSerializable(typeof(Program))]
partial class IntegrationEventContext : JsonSerializerContext;