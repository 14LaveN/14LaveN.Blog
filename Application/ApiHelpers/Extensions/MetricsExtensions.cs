using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Prometheus.Client.HttpRequestDurations;

namespace Application.ApiHelpers.Extensions;

public static class MetricsExtensions
{
    public static IApplicationBuilder UseMetricsExtension(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        
        app.UseMetricServer()
            .UseHttpMetrics()
            .UsePrometheusRequestDurations();

        return app;
    }
}