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
        string cacheKey = $"product:{productId}";
        string? cachedProduct = await _cache.GetStringAsync(cacheKey);

        if (cachedProduct != null)
        {
            try
            {
                ProductDto? productFromCache = JsonSerializer.Deserialize<ProductDto>(cachedProduct);

                if (productFromCache is not null)
                {
                    _logger.LogInformation("Product {ProductId} found in cache", productId);
                    return productFromCache;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Product {ProductId} not found in cache", productId);
            }
        }

        try
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync($"/gateway/products/search/product-id/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    ProductDto? productFromFallback = await response.Content.ReadFromJsonAsync<ProductDto>() ??
                                                      throw new NotImplementedException(
                                                          "Fallback policy was not implemented");
                    return productFromFallback;
                }

                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => null,
                    HttpStatusCode.BadRequest => throw new HttpRequestException("Bad request", null,
                        HttpStatusCode.BadRequest),
                    _ => throw new HttpRequestException(
                        $"Http request failed with status code {response.StatusCode}")
                };
            }

            ProductDto? product = await response.Content.ReadFromJsonAsync<ProductDto>() ??
                                  throw new ArgumentException("Invalid Product ID");

            string productJson = JsonSerializer.Serialize(product);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                .SetSlidingExpiration(TimeSpan.FromSeconds(100));

            await _cache.SetStringAsync(cacheKey, productJson, options);

            return product;
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