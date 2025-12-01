using BusinessLogicLayer.DTO;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients;

public sealed class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _cache;

    public ProductsMicroserviceClient(
        HttpClient httpClient,
        ILogger<ProductsMicroserviceClient> logger,
        IDistributedCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductDto?> GetProductByProductIdAsync(Guid productId)
    {
        try
        {
            string cacheKey = $"product:{productId}";
            string? cachedProduct = await _cache.GetStringAsync(cacheKey);

            if (cachedProduct != null)
            {
                ProductDto? productFromCache = JsonSerializer.Deserialize<ProductDto>(cachedProduct);
                return productFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => null,
                    HttpStatusCode.BadRequest => throw new HttpRequestException("Bad request", null,
                        HttpStatusCode.BadRequest),
                    _ => throw new HttpRequestException($"Http request failed with status code {response.StatusCode}")
                };
            }

            ProductDto? product = await response.Content.ReadFromJsonAsync<ProductDto>();

            return product ?? throw new ArgumentException("Invalid Product ID");
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full");

            return new ProductDto(
                ProductID: Guid.NewGuid(),
                ProductName: "Temporarily Unavailable (Bulkhead)",
                Category: "Temporarily Unavailable (Bulkhead)",
                UnitPrice: 0,
                QuantityInStock: 0);
        }
    }
}