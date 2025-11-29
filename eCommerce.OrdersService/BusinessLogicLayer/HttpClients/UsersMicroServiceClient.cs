using BusinessLogicLayer.DTO;
using System.Net;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients;

public sealed class UsersMicroServiceClient
{
    private readonly HttpClient _httpClient;

    public UsersMicroServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userId}", cancellationToken);

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

        UserDto? user = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken);

        if (user is null)
        {
            throw new ArgumentNullException("Invalid user id");
        }

        return user;
    }
}
