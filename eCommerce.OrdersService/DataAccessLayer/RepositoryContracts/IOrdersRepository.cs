using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.RepositoryContracts;

/// <summary>
/// Represents methods to be implemented by Orders Repo
/// </summary>
public interface IOrdersRepository
{
    /// <summary>
    /// Retrieves all Orders async
    /// </summary>
    /// <returns>IEnumerale of <see cref="Order"/></returns>
    public Task<IEnumerable<Order>> GetOrdersAsync();

    /// <summary>
    /// Retrieves all Orders by condition async
    /// </summary>
    /// <returns>IEnumerale of <see cref="Order"/></returns>
    public Task<IEnumerable<Order?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter);

    /// <summary>
    /// Retrieves a single Order by condition async
    /// </summary>
    /// <returns>Returnns <see cref="Order"/> or null</returns>
    public Task<Order?> GetOrderByConditionAsync(FilterDefinition<Order> filter);

    /// <summary>
    /// Adds a new Order into the Orders collection
    /// </summary>
    /// <param name="order"><see cref="Order"/></param>
    /// <returns>Returns <see cref="Order"/> or null</returns>
    public Task<Order?> AddOrderAsync(Order order);

    /// <summary>
    /// Updates an existing Order
    /// </summary>
    /// <param name="order"><see cref="Order"/></param>
    /// <returns>Returns <see cref="Order"/> or null</returns>
    public Task<Order?> UpdateOrderAsync(Order order);

    /// <summary>
    /// Deletes an existing Order
    /// </summary>
    /// <param name="id">Order id</param>
    /// <returns>Returns true if order is deleted otherwise false</returns>
    public Task<bool> DeleteOrderAsync(Guid id);
}
