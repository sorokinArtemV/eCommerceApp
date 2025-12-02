using BusinessLogicLayer.DTO;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BusinessLogicLayer.HttpClients;

public sealed class UsersMicroServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroServiceClient> _logger;
    private readonly IDistributedCache _cache;

    public UsersMicroServiceClient(
        HttpClient httpClient,
        ILogger<UsersMicroServiceClient> logger,
        IDistributedCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"user:{userId}";
        string? cachedUser = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (cachedUser is not null)
        {
            try
            {
                UserDto? userFromCache = JsonSerializer.Deserialize<UserDto>(cachedUser);

                if (userFromCache is not null)
                {
                    _logger.LogInformation("User {UserId} found in cache", userId);
                    return userFromCache;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached user {UserId}. Ignoring cache.", userId);
            }
        }

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

            UserDto? user = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken)
                            ?? throw new ArgumentException("Invalid User ID");

            string userJson = JsonSerializer.Serialize(user);

            DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            await _cache.SetStringAsync(cacheKey, userJson, distributedCacheEntryOptions, cancellationToken);

            return user;
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