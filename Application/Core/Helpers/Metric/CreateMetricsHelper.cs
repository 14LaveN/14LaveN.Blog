using System.Diagnostics;
using System.Globalization;
using Application.Core.Abstractions.Redis;
using Application.Core.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Application.Core.Helpers.Metric;

/// <summary>
/// Represents the create metrics helper.
/// </summary>
public sealed class CreateMetricsHelper(
    IDistributedCache distributedCache) 
{
    private static readonly Counter RequestCounter =
        Metrics.CreateCounter("AiSearch_requests_total", "Total number of requests.");

    /// <summary>
    /// Create metrics method.
    /// </summary>
    /// <param name="stopwatch">The <see cref="Stopwatch"/> class.</param>
    public async Task CreateMetrics(Stopwatch stopwatch)
    {
        RequestCounter.Inc();

        Metrics.CreateHistogram("AiSearch_request_duration_seconds", "Request duration in seconds.")
            .Observe(stopwatch.Elapsed.TotalMilliseconds);

        Dictionary<string, string> counter = new Dictionary<string, string>()
        {
            {"Name", RequestCounter.Name},
            {"Value",RequestCounter.Value.ToString(CultureInfo.InvariantCulture)}
        };
        
        await distributedCache.SetRecordAsync(
            "metrics_counter-key",
            counter,
            TimeSpan.FromMinutes(6),
            TimeSpan.FromMinutes(6));

        await distributedCache.SetRecordAsync(
            "metrics_request_duration_seconds-key",
            stopwatch.Elapsed.TotalMilliseconds.ToString(CultureInfo.CurrentCulture),
            TimeSpan.FromMinutes(6),
            TimeSpan.FromMinutes(6));
    }
}