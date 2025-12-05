namespace BusinessLogicLayer.RabbitMQ.ConnectionService;

public class RabbitMqOptions
{
    public required string HostName { get; set; }
    public int Port { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public string VirtualHost { get; set; } = "/";

    public string Exchange { get; set; } = "my-exchange";
    public string ExchangeType { get; set; } = "direct";
    public string Queue { get; set; } = "my-queue";
    public string RoutingKey { get; set; } = "my-routing-key";
}
