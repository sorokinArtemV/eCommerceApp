namespace BusinessLogicLayer.RabbitMQ;

public record ProductDeletedMessage(
    Guid ProductId,
    DateTimeOffset PublishedAt
);