using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public sealed class OrderItemUdateRequestValidator : AbstractValidator<OrderItemUpdateRequest>
{
    public OrderItemUdateRequestValidator()
    {
        RuleFor(x => x.ProductID).NotEmpty().WithMessage("Product ID can not be empty");

        RuleFor(x => x.UnitPrice).NotEmpty().WithMessage("Unit price can not be empty")
            .GreaterThan(0).WithMessage("Unit price must be greater than 0");

        RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity can not be empty")
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}
