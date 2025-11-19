using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Validators.ValidatorExtensions;

public static class ValidationResultExtensions
{
    public static IResult ToValidationResult(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return Results.ValidationProblem(errors);
    }
}