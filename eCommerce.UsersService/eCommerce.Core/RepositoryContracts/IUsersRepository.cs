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

    /// <summary>
    /// Method to return user by its id
    /// </summary>
    /// <param name="userId">guid id</param>
    /// <returns><see cref="ApplicationUser"/> or null</returns>
    public Task<ApplicationUser?> GetUserByUserId(Guid? userId);
}