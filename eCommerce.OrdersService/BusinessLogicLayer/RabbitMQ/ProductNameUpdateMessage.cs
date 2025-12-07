namespace BusinessLogicLayer.RabbitMQ;

public record ProductNameUpdateMessage(
    Guid ProductId,
    string? NewProductName,
    DateTimeOffset PublishedAt
    );
