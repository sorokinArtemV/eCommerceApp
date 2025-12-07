using BusinessLogicLayer.RabbitMQ.ConnectionService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ.Publisher;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IRabbitMqConnectionAccessor _accessor;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly Dictionary<string, string> _routes;


    public RabbitMqPublisher(
        IRabbitMqConnectionAccessor accessor,
        IOptions<RabbitMqPublisherOptions> publisherOptions,
        ILogger<RabbitMqPublisher> logger)
    {
        _accessor = accessor;
        _logger = logger;
        _routes = publisherOptions.Value.Routes;
    }

    public Task PublishNameUpdatedAsync(ProductNameUpdateMessage message, CancellationToken ct = default)
    => PublishInternalAsync("ProductNameUpdated", message, ct);

    public Task PublishDeletedAsync(ProductDeletedMessage message, CancellationToken ct = default)
        => PublishInternalAsync("ProductDeleted", message, ct);

    private async Task PublishInternalAsync<T>(string routeName, T message, CancellationToken cancellationToken = default)
    {
        if (!_routes.TryGetValue(routeName, out var routingKey))
            throw new InvalidOperationException($"Unknown route '{routeName}'");

        IChannel channel = await _accessor.GetChannelAsync(cancellationToken);
        RabbitMqOptions options = _accessor.Options;

        byte[] body = JsonSerializer.SerializeToUtf8Bytes(message);

        BasicProperties properties = new()
        {
            Persistent = true,
            ContentType = "application/json"
        };


        _logger.LogInformation("Publishing message to exchange '{Exchange}' with routing key '{RoutingKey}'", options.Exchange, routingKey);

        await channel.BasicPublishAsync(
            exchange: options.Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
