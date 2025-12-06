using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMqProductNameUpdateConsumer : BackgroundService
{
    private readonly IRabbitMqConnectionAccessor _connectionAccessor;
    private readonly ILogger<RabbitMqProductNameUpdateConsumer> _logger;

    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqProductNameUpdateConsumer(
        IRabbitMqConnectionAccessor connectionAccessor,
        ILogger<RabbitMqProductNameUpdateConsumer> logger)
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

        // Берём живой канал через наш connection service
        IChannel channel = await _connectionAccessor.GetChannelAsync(stoppingToken);
        RabbitMqOptions options = _connectionAccessor.Options;

        // QoS: ограничиваем количество "висящих" сообщений
        ushort prefetch = options.PrefetchCount;

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: prefetch,
            global: false,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Configured BasicQos with prefetch={Prefetch} for queue {Queue}",
            prefetch,
            options.Queue);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            try
            {
                ReadOnlyMemory<byte> body = ea.Body;

                ProductNameUpdateMessage? message =
                    JsonSerializer.Deserialize<ProductNameUpdateMessage>(body.Span, _jsonOptions);

                if (message is null)
                {
                    _logger.LogWarning(
                        "Failed to deserialize ProductNameUpdateMessage. DeliveryTag={DeliveryTag}",
                        ea.DeliveryTag);

                    await channel.BasicNackAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                // тут твоя Orders-логика: обновить read-модель, кэш и т.д.
                _logger.LogInformation(
                    "Received ProductNameUpdateMessage: ProductId={ProductId}, NewName={NewName}, delivery took {DeliveryTime}",
                    message.ProductId,
                    message.NewProductName,
                    DateTimeOffset.UtcNow - message.PublishedAt
                    );

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

        // Подписываемся на очередь Orders (options.Queue = "orders.product-name-updated")
        string consumerTag = await channel.BasicConsumeAsync(
            queue: options.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Started consuming queue {Queue} with consumerTag={ConsumerTag}",
            options.Queue,
            consumerTag);

        // Держим сервис живым до остановки
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
