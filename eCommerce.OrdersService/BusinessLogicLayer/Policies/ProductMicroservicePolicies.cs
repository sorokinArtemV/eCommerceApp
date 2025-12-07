using System.Net;
using System.Text;
using System.Text.Json;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;

namespace BusinessLogicLayer.Policies;

public class ProductMicroservicePolicies
{
    private readonly ILogger<ProductMicroservicePolicies> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _fallbackPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _bulkheadPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _allPolicies;

    public ProductMicroservicePolicies(ILogger<ProductMicroservicePolicies> logger)
    {
        _logger = logger;

        _fallbackPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(context =>
            {
                _logger.LogInformation("Fallback Policy triggered: the request failed");

                ProductDto productDto = new ProductDto(ProductID: Guid.Empty,
                    ProductName: "Temporarily Unavailable(fallback)",
                    Category: "Temporarily Unavailable(fallback)",
                    UnitPrice: 0.0,
                    QuantityInStock: 0
                );

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent(JsonSerializer.Serialize(productDto),
                        Encoding.UTF8, "application/json")
                });
            });

        _bulkheadPolicy = Policy
            .BulkheadAsync<HttpResponseMessage>(
                maxParallelization: 10, maxQueuingActions: 20, onBulkheadRejectedAsync: context =>
                {
                    _logger.LogWarning("Bulkhead Policy triggered: too many concurrent requests");
                    throw new BulkheadRejectedException("Too many concurrent requests to the Product Microservice");
                });

        _allPolicies = Policy.WrapAsync(_fallbackPolicy, _bulkheadPolicy);
    }

    public IAsyncPolicy<HttpResponseMessage> Fallback => _fallbackPolicy;
    public IAsyncPolicy<HttpResponseMessage> Bulkhead => _bulkheadPolicy;
    public IAsyncPolicy<HttpResponseMessage> AllPolicies => _allPolicies;
}