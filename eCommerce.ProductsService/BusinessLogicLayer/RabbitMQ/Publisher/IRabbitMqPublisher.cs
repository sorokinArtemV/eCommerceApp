namespace BusinessLogicLayer.RabbitMQ.Publisher;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default);
}
