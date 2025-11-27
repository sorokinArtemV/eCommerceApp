using eCommerce.Core.DTO;

namespace eCommerce.Core.ServiceContracts;

/// <summary>
/// Contract for user service 
/// </summary>
public interface IUsersService
{
    /// <summary>
    /// Method to handle user login
    /// </summary>
    public Task<AuthenticationResponse?> Login(LoginRequest loginRequest);

    /// <summary>
    /// Method to handle user registration
    /// </summary>
    public Task<AuthenticationResponse?> Register(RegisterRequest registerRequest);

    /// <summary>
    /// Method to get user by its id
    /// </summary>
    public Task<UserDto?> GetUserById(Guid? userId);
}