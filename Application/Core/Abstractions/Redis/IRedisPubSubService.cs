using StackExchange.Redis;

namespace Application.Core.Abstractions.Redis;

public interface 
    IRedisPubSubService
{
    Task SubscribeAsync(
        string channel,
        Action<RedisChannel, RedisValue> handler);
    
    Task PublishAsync(
        string channel,
        string message);
}