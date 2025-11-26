using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace BusinessLogicLayer.ServiceContracts;


/// <summary>
/// Represents contracts for Order Service
/// </summary>
public interface IOrdersService
{
    /// <summary>
    /// Retrieves the list of orders from orders repository async
    /// </summary>
    /// <returns>Returns a collection <see cref="OrderResponse"/></returns>
    Task<List<OrderResponse?>> GetOrdersAsync();

    /// <summary>
    /// Retrieves the list of orders from orders repository by filter async
    /// </summary>
    /// <returns>Returns a collection <see cref="OrderResponse"/></returns>
    Task<List<OrderResponse?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter);

    /// <summary>
    /// Retrieves a single order from orders repository by filter async
    /// </summary>
    /// <returns>Returns a single <see cref="OrderResponse"/> or null</returns>
    Task<OrderResponse?> GetOrderByConditionAsync(FilterDefinition<Order> filter);

    /// <summary>
    /// Adds an order to the Orders reository
    /// </summary>
    /// <param name="orderAddRequest"></param>
    /// <returns>Returns added <see cref="OrderResponse"/> or null</returns>
    Task<OrderResponse?> AddOrderAsync(OrderAddRequest? orderAddRequest);

    /// <summary>
    /// Updates an order in the Orders reository
    /// </summary>
    /// <param name="orderUpdateRequest"></param>
    /// <returns>Returns updated <see cref="OrderResponse"/> or null</returns>
    Task<OrderResponse?> UpdateOrderAsync(OrderUpdateRequest? orderUpdateRequest);

    /// <summary>
    /// Deletes an existing order based on given order id
    /// </summary>
    /// <param name="orderId">OrderID to search</param>
    /// <returns>Return true if deleted else false</returns>
    Task<bool> DeleteOrderAsync(Guid orderId);
}
