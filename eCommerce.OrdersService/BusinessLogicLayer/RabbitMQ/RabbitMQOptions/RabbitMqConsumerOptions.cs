namespace BusinessLogicLayer.RabbitMQ.RabbitMQOptions;

public class RabbitMqConsumerOptions
{
    public ushort PrefetchCount { get; set; } = 20;

    public int RetryCount { get; set; } = 5;
    public int RetryDelaySeconds { get; set; } = 5;

    public List<ConsumerQueueOptions> Queues { get; set; } = [];
}