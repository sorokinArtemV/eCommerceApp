namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMqOptions
{

    public string HostName { get; set; } = default!;
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string Exchange { get; set; } = default!;
    public string ExchangeType { get; set; } = "direct";
    public string RoutingKey { get; set; } = default!;
    public string Queue { get; set; } = default!;


    public ushort PrefetchCount { get; set; } = 20;


    public int RetryCount { get; set; } = 5;
    public int RetryDelaySeconds { get; set; } = 5;

    public int HeartbeatSeconds { get; set; } = 30;
}
