namespace BusinessLogicLayer.RabbitMQ.RabbitMQOptions;

public class ConsumerQueueOptions
{
    public required string Queue { get; set; }
    public required string RoutingKey { get; set; }
}