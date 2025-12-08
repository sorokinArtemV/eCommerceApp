using DataAccessLayer.Entities;

namespace BusinessLogicLayer.RabbitMQ.Publisher;

public interface IRabbitMqPublisher
{
    public Task PublishProductUpdatedAsync(Product product, CancellationToken ct = default);
    public Task PublishProductDeletedAsync(Product product, CancellationToken ct = default);
}
