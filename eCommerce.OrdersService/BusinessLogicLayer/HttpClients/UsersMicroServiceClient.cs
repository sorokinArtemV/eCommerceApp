using BusinessLogicLayer.DTO;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BusinessLogicLayer.HttpClients;

public sealed class UsersMicroServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroServiceClient> _logger;

    public UsersMicroServiceClient(HttpClient httpClient, ILogger<UsersMicroServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => null,
                    HttpStatusCode.BadRequest => throw new HttpRequestException("Bad request", null,
                        HttpStatusCode.BadRequest),
                    _ => new UserDto(PersonName: "Temporarily Unavailable", Email: "Temporarily Unavailable",
                        Gender: "Temporarily Unavailable", UserID: Guid.Empty)
                };
            }

            UserDto? user = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken);

            return user ?? throw new ArgumentException("Invalid User ID");
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request failed because of circuit breaker is in Open state");

            return new UserDto(
                PersonName: "Temporarily Unavailable(circuit breaker)",
                Email: "Temporarily Unavailable(circuit breaker)",
                Gender: "Temporarily Unavailable(circuit breaker)",
                UserID: Guid.Empty);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Request timed out");

            return new UserDto(
                PersonName: "Temporarily Unavailable(timeout)",
                Email: "Temporarily Unavailable(timeout)",
                Gender: "Temporarily Unavailable(timeout)",
                UserID: Guid.Empty);
        }
    }
}