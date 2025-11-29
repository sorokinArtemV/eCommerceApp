using BusinessLogicLayer.DTO;
using System.Net;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients;

public sealed class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;

    public ProductsMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDto?> GetProductByProductIdAsync(Guid productId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productId}");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
            }
            else
            {
                throw new HttpRequestException("Error retrieving user", null, response.StatusCode);
            }
        }

        ProductDto? product = await response.Content.ReadFromJsonAsync<ProductDto>();

        if (product is null)
        {
            throw new ArgumentNullException("Invalid product id");
        }

        return product;
    }
}