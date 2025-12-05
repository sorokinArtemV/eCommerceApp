using BusinessLogicLayer.RabbitMQ.ConnectionService;
using RabbitMQ.Client;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IRabbitMqConnectionAccessor _accessor;

    public RabbitMqPublisher(IRabbitMqConnectionAccessor accessor)
    {
        _accessor = accessor;
    }

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        var channel = _accessor.Channel;
        var options = _accessor.Options;

        byte[] body = JsonSerializer.SerializeToUtf8Bytes(message);

        BasicProperties properties = new()
        {
            Persistent = true,
            ContentType = "application/json"
        };

        var effectiveRoutingKey = string.IsNullOrEmpty(routingKey)
            ? options.RoutingKey
            : routingKey;

        await channel.BasicPublishAsync(
            exchange: options.Exchange,
            routingKey: effectiveRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

    }
}
