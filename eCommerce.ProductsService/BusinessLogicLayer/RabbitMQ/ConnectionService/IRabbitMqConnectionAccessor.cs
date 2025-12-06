using RabbitMQ.Client;

namespace BusinessLogicLayer.RabbitMQ.ConnectionService;

public interface IRabbitMqConnectionAccessor
{
    RabbitMqOptions Options { get; }

    /// <summary>
    /// Возвращает живой канал. При необходимости пересоздаёт connection/канал.
    /// </summary>
    ValueTask<IChannel> GetChannelAsync(CancellationToken cancellationToken = default);
}
