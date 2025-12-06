using RabbitMQ.Client;

namespace BusinessLogicLayer.RabbitMQ;

public interface IRabbitMqConnectionAccessor
{
    RabbitMqOptions Options { get; }
    ValueTask<IChannel> GetChannelAsync(CancellationToken cancellationToken = default);
}
