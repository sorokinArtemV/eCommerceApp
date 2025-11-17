using eCommerce.Core.Entities;

namespace eCommerce.Core.RepositoryContracts;

/// <summary>
/// Contract to be implemented by User Repository
/// </summary>
public interface IUsersRepository
{
    /// <summary>
    /// Method to add user to the data store
    /// </summary>  
    public Task<ApplicationUser?> AddUser(ApplicationUser user);

    /// <summary>
    /// Method to get user by email and password
    /// </summary>
    public Task<ApplicationUser?> GetUserByEmailAndPassword(string? email, string? password);
}