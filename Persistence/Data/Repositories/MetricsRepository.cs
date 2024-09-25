using Application.Core.Abstractions;
using Domain.Common.Core.Primitives.Maybe;
using Domain.Entities;
using Domain.Repositories;
using MongoDB.Driver;

namespace Persistence.Data.Repositories;

/// <summary>
/// Represents the generic metrics repository class.
/// </summary>
internal sealed class MetricsRepository
    : IMongoRepository<MetricEntity>, IMetricsRepository
{
    private readonly ICommonMongoDbContext _commonMongoDbContext;

    /// <summary>
    /// Login new instance of metrics repository.
    /// </summary>
    /// <param name="commonMongoDbContext">The common mongo db context.</param>
    public MetricsRepository(
        ICommonMongoDbContext commonMongoDbContext)
    {
        _commonMongoDbContext = commonMongoDbContext;
    }

    /// <inheritdoc />
    public async Task<List<MetricEntity>> GetAllAsync() =>
        await _commonMongoDbContext.Metrics.Find(_ => true).ToListAsync();

    /// <inheritdoc />
    public async System.Threading.Tasks.Task InsertAsync(MetricEntity type) =>
        await _commonMongoDbContext.Metrics.InsertOneAsync(type);

    /// <inheritdoc />
    public async Task<Maybe<List<MetricEntity>>> GetByTime(int time, string? metricName)
    {
        var metrics = await _commonMongoDbContext.Metrics
            .FindAsync(x=>
                x.Created_At.Day == DateTime.Today.Day 
                && x.Created_At.Month == DateTime.Today.Month
                && x.Created_At.Year == DateTime.Today.Year
                && x.Name == metricName)
            .Result
            .ToListAsync();
        
        if (metrics.Count is 0)
            return Maybe<List<MetricEntity>>.None;

        return metrics;
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task InsertRangeAsync(IEnumerable<MetricEntity> types) =>
        await _commonMongoDbContext.Metrics.InsertManyAsync(types);

    /// <inheritdoc />
    public async System.Threading.Tasks.Task RemoveAsync(string id) =>
        await _commonMongoDbContext.Metrics.DeleteOneAsync(x => x.Id == id);
}