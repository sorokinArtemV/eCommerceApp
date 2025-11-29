namespace BusinessLogicLayer.DTO;

public record OrderItemAddRequest(
    Guid ProductID,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    string? ProductName,
    string? Category
    )
{
    public OrderItemAddRequest() : this(default, default, default, default, default, default)
    {

    }
}
