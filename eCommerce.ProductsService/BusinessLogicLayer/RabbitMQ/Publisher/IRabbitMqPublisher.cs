namespace BusinessLogicLayer.RabbitMQ.Publisher;

public interface IRabbitMqPublisher
{
    public Task PublishNameUpdatedAsync(ProductNameUpdateMessage message, CancellationToken ct = default);
    public Task PublishDeletedAsync(ProductDeletedMessage message, CancellationToken ct = default);
}
