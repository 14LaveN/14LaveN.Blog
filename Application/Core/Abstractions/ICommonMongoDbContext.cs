using Domain.Entities;
using MongoDB.Driver;

namespace Application.Core.Abstractions;

/// <summary>
/// The common mongo database context interface.
/// </summary>
public interface ICommonMongoDbContext
{
    /// <summary>
    /// Gets metrics mongo collection.
    /// </summary>
    IMongoCollection<MetricEntity> Metrics { get; }

    /// <summary>
    /// Gets rabbit messages mongo collection.
    /// </summary>
    IMongoCollection<RabbitMessage> RabbitMessages { get; }
}