using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using BusinessLogicLayer.RabbitMQ.RabbitMQOptions;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMqProductEventsConsumer : BackgroundService
{
    private readonly IRabbitMqConnectionAccessor _connectionAccessor;
    private readonly ILogger<RabbitMqProductEventsConsumer> _logger;

    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqProductEventsConsumer(
        IRabbitMqConnectionAccessor connectionAccessor,
        ILogger<RabbitMqProductEventsConsumer> logger)
    {
        _connectionAccessor = connectionAccessor;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting RabbitMqProductNameUpdateConsume...");

        IChannel channel = await _connectionAccessor.GetChannelAsync(stoppingToken);
        RabbitMqOptions options = _connectionAccessor.Options;
        RabbitMqConsumerOptions consumerOptions = _connectionAccessor.ConsumerOptions;

        ushort prefetch = consumerOptions.PrefetchCount;

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: prefetch,
            global: false,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Configured BasicQos with prefetch={Prefetch} for queue {Queue}",
            prefetch,
            string.Join(", ", consumerOptions.Queues));

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            try
            {
                ReadOnlyMemory<byte> body = ea.Body;
                var routingKey = ea.RoutingKey;

                switch (routingKey)
                {
                    case "product.name.updated":
                    {
                        var message = JsonSerializer.Deserialize<ProductNameUpdateMessage>(body.Span, _jsonOptions);
                        if (message is null)
                        {
                            throw new InvalidOperationException("Failed to deserialize ProductNameUpdateMessage");
                        }

                        _logger.LogInformation(
                            "Name updated: ProductId={ProductId}, NewName={NewName}, delivery took {DeliveryTime}",
                            message.ProductId,
                            message.NewProductName,
                            DateTimeOffset.UtcNow - message.PublishedAt
                        );

                        break;
                    }

                    case "product.deleted":
                    {
                        var message = JsonSerializer.Deserialize<ProductDeletedMessage>(body.Span, _jsonOptions);
                        if (message is null)
                        {
                            throw new InvalidOperationException("Failed to deserialize ProductDeletedMessage");
                        }

                        _logger.LogInformation(
                            "Product deleted: ProductId={ProductId}, delivery took {DeliveryTime}",
                            message.ProductId,
                            DateTimeOffset.UtcNow - message.PublishedAt
                        );

                        break;
                    }

                    default:
                        _logger.LogWarning("Unknown routing key {RoutingKey}, DeliveryTag={DeliveryTag}",
                            routingKey, ea.DeliveryTag);
                        break;
                }

                await channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing ProductNameUpdateMessage. DeliveryTag={DeliveryTag}",
                    ea.DeliveryTag);

                await channel.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        foreach (var queue in consumerOptions.Queues)
        {
            string consumerTag = await channel.BasicConsumeAsync(
                queue: queue.Queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                "Started consuming queue {Queue} with consumerTag={ConsumerTag}",
                queue.Queue,
                consumerTag);
        }

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // нормальный shutdown
        }

        _logger.LogInformation("Stopping RabbitMqProductNameUpdateConsume...");
    }
}