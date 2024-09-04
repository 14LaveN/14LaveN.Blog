using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions;

public interface IEventBusBuilder
{
    public IServiceCollection Services { get; }
}
