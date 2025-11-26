using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public sealed class OrderAddRequestValidator : AbstractValidator<OrderAddRequest>
{
    public OrderAddRequestValidator(IValidator<OrderItemAddRequest> orderItemValidator)
    {
        RuleFor(x => x.UserID).NotEmpty().WithMessage("User ID can not be empty");
        RuleFor(x => x.OrderDate).NotEmpty().WithMessage("Order date can not be empty");
        RuleFor(x => x.OrderItems).NotEmpty().WithMessage("Order items can not be empty");
        RuleForEach(x => x.OrderItems).SetValidator(orderItemValidator);
    }
}
