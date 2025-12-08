namespace BusinessLogicLayer.RabbitMQ.ConnectionService;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public required string HostName { get; set; }
    public int Port { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string VirtualHost { get; set; }

    public required string Exchange { get; set; }
    public required string ExchangeType { get; set; }
    public required string Queue { get; set; }
    public required string RoutingKey { get; set; }
}
