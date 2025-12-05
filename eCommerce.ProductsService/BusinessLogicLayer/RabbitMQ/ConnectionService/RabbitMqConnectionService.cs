using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BusinessLogicLayer.RabbitMQ.ConnectionService;

public class RabbitMqConnectionService : IHostedService, IRabbitMqConnectionAccessor, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IChannel? _channel;

    public IConnection Connection => _connection ??
        throw new InvalidOperationException("RabbitMQ connection not initialized yet");

    public IChannel Channel => _channel ??
        throw new InvalidOperationException("RabbitMQ channel not initialized yet");

    public RabbitMqOptions Options => _options;

    public RabbitMqConnectionService(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = new()
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: _options.ExchangeType,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: _options.Queue,
            exchange: _options.Exchange,
            routingKey: _options.RoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            try
            {
                await _channel.CloseAsync();
            }
            catch
            {
            }

            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync();
            }
            catch
            {
            }

            await _connection.DisposeAsync();
        }
    }
}
