using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        // TODO: Add Data Business logic Layer services into the IoC container
        services.AddAutoMapper(_ => { }, typeof(ProductAddRequestToProductMappingProfile));

        services.AddScoped<IProductsService, ProductsService>();
        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();

        return services;
    }
}