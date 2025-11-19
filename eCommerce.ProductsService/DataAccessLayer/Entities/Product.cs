using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public sealed class Product
{
    [Key]
    public Guid ProductId { get; set; }

    public required string ProductName { get; set; }
    public required string Category { get; set; }
    public double? UnitPrice { get; set; }
    public int? QuantityInStock { get; set; }
}