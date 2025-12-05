using RabbitMQ.Client;

namespace BusinessLogicLayer.RabbitMQ.ConnectionService;

public interface IRabbitMqConnectionAccessor
{
    public IConnection Connection { get; }
    public IChannel Channel { get; }
    RabbitMqOptions Options { get; }
}
