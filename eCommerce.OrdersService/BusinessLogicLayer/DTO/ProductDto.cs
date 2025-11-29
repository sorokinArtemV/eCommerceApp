namespace BusinessLogicLayer.DTO;

public record ProductDto(
    Guid ProductID,
    string? ProductName,
    string? Category,
    double UnitPrice,
    int QuantityInStock
    );

