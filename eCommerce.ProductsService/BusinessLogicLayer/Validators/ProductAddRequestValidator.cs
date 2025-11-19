using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public sealed class ProductAddRequestValidator : AbstractValidator<ProductAddRequest>
{
    public ProductAddRequestValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().WithMessage("Product Name is required.");

        RuleFor(x => x.Category).IsInEnum().WithMessage("Category is invalid.");

        RuleFor(x => x.UnitPrice).InclusiveBetween(0, double.MaxValue)
            .WithMessage($"Unit Price is must be between 0 and {double.MaxValue}.");

        RuleFor(x => x.QuantityInStock).InclusiveBetween(0, int.MaxValue)
            .WithMessage($"Quantity must be between 0 and {int.MaxValue}.");
    }
}