using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BusinessLogicLayer.RabbitMQ.ConnectionService;

public class RabbitMqConnectionService : IHostedService, IRabbitMqConnectionAccessor, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _syncRoot = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;

    public RabbitMqOptions Options => _options;

    public RabbitMqConnectionService(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await EnsureConnectedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return DisposeAsync().AsTask();
    }

    public async ValueTask<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_channel is { IsOpen: true })
            return _channel;

        await EnsureConnectedAsync(cancellationToken);

        return _channel!;
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            return;

        await _syncRoot.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
                return;

            await CleanupAsync();

            _connection = await ConnectWithRetryAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await ConfigureTopologyAsync(_channel, cancellationToken);
        }
        finally
        {
            _syncRoot.Release();
        }
    }

    private async Task ConfigureTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.QueueDeclareAsync(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.Queue,
            exchange: _options.Exchange,
            routingKey: _options.RoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task CleanupAsync()
    {
        if (_channel is not null)
        {
            try
            {
                if (_channel.IsOpen)
                    await _channel.CloseAsync();
            }
            catch
            {

            }

            await _channel.DisposeAsync();
            _channel = null;
        }

        if (_connection is not null)
        {
            try
            {
                if (_connection.IsOpen)
                    await _connection.CloseAsync();
            }
            catch
            {
            }

            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private async Task<IConnection> ConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = new()
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            // heartbeat / tuning
        };

        const int MaxAttempts = 10;
        var delay = TimeSpan.FromSeconds(5);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= MaxAttempts && !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                return await factory.CreateConnectionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == MaxAttempts)
                    break;

                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Could not connect to RabbitMQ after {MaxAttempts} attempts.", lastException);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;
        await CleanupAsync();
        _syncRoot.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMqConnectionService));
        }
    }
}