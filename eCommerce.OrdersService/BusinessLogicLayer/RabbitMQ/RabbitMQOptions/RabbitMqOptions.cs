namespace BusinessLogicLayer.RabbitMQ.RabbitMQOptions;

public class RabbitMqOptions
{
    public string HostName { get; set; } = default!;
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string Exchange { get; set; } = default!;
    public string ExchangeType { get; set; } = "topic";

    public int HeartbeatSeconds { get; set; } = 30;
}