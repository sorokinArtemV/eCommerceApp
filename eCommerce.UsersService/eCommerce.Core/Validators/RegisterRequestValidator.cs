using eCommerce.Core.DTO;
using FluentValidation;

namespace eCommerce.Core.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotNull().WithMessage("Email is required");
        RuleFor(x => x.Password).NotNull().WithMessage("Password is required");
        RuleFor(x => x.PersonName).NotNull().WithMessage("PersonName is required");
        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Gender must be one of: Male, Female, Other");
    }
}