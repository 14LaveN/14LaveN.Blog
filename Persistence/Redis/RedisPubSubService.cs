using Application.Core.Abstractions.Redis;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Persistence.Redis;

internal sealed class RedisPubSubService(IConnectionMultiplexer connectionMultiplexer)
    : IRedisPubSubService
{
    public async Task SubscribeAsync(
        string channel,
        Action<RedisChannel, RedisValue> handler)
    {
        ISubscriber subscriber = connectionMultiplexer.GetSubscriber();
        await subscriber.SubscribeAsync(channel, handler);
    }

    public async Task PublishAsync(string channel, string message)
    {
        ISubscriber subscriber = connectionMultiplexer.GetSubscriber();
        await subscriber.PublishAsync(channel, message);
    }
}