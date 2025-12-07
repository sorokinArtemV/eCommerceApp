using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Validators.ValidatorExtensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.API.ApiEndpoints;

public static class ProductApiEndpoints
{
    public static IEndpointRouteBuilder MapProductApiEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/products
        app.MapGet("api/products", async (IProductsService productsService, CancellationToken ct) =>
        {
            List<ProductResponse?> products = await productsService.GetProductsAsync();
            return Results.Ok(products);
        });

        // GET /api/products/search/product-id/xxx-xx-xxx-xxx
        app.MapGet("/api/products/search/product-id/{productId:guid}",
            async (IProductsService productsService, Guid productId, CancellationToken ct) =>
            {
                ProductResponse? productResponse = await productsService
                    .GetProductByConditionAsync(x => x.ProductId == productId);

                // TO REMOVE: TESTING PURPOSES ONLY
                // await Task.Delay(1000, ct);
                // throw new NotImplementedException();

                if (productResponse is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(productResponse);
            });

        // GET /api/products/search/xxxx
        app.MapGet("/api/products/search/{searchString}",
            async (IProductsService productsService, string searchString, CancellationToken ct) =>
            {
                List<ProductResponse?> productsByName = await productsService.GetProductsByConditionAsync(x =>
                    x.ProductName.Contains(searchString, StringComparison.OrdinalIgnoreCase));

                List<ProductResponse?> productsByCategory = await productsService.GetProductsByConditionAsync(x =>
                    x.Category.Contains(searchString, StringComparison.OrdinalIgnoreCase));

                IEnumerable<ProductResponse?> allProducts = productsByName.Union(productsByCategory);

                return Results.Ok(allProducts);
            });

        // POST /api/products
        app.MapPost("/api/products", async (
            IProductsService productsService,
            IValidator<ProductAddRequest> validator,
            [FromBody] ProductAddRequest request,
            CancellationToken ct) =>
        {
            ValidationResult? validationResult = await validator.ValidateAsync(request, ct);

            if (!validationResult.IsValid) return validationResult.ToValidationResult();

            ProductResponse? productResponse = await productsService.AddProductAsync(request);

            return productResponse is null
                ? Results.Problem("Failed to create product")
                : Results.Created($@"api/products/search/product-id/{productResponse.ProductId}", productResponse);
        });

        // PUT /api/products
        app.MapPut("/api/products", async (
            IProductsService productsService,
            IValidator<ProductUpdateRequest> validator,
            ProductUpdateRequest request,
            CancellationToken ct) =>
        {
            ValidationResult? validationResult = await validator.ValidateAsync(request, ct);

            if (!validationResult.IsValid) return validationResult.ToValidationResult();

            ProductResponse? productResponse = await productsService.UpdateProductAsync(request);

            return productResponse is null
                ? Results.Problem("Failed to update product")
                : Results.Ok(productResponse);
        });

        // DELETE /api/products/xxx-xxx-xx-xxxx
        app.MapDelete("/api/products/{productId:guid}", async (
            IProductsService productsService,
            Guid productId,
            CancellationToken ct) =>
        {
            bool isDeleted = await productsService.DeleteProductAsync(productId);

            return isDeleted ? Results.Ok(true) : Results.Problem("Failed to delete product");
        });

        return app;
    }
}