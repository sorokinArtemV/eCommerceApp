using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    [HttpGet] // GET /api/orders
    public async Task<IEnumerable<OrderResponse?>> GetOrdersAsync()
    {
        List<OrderResponse?> orders = await _ordersService.GetOrdersAsync();
        return orders;
    }


    [HttpGet("search/orderid/{orderId}")] // GET /api/orders/search/orderid/{orderID}
    public async Task<OrderResponse?> GetOrderByIdAsync(Guid orderId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderID, orderId);
        OrderResponse? order = await _ordersService.GetOrderByConditionAsync(filter);

        return order;
    }

    [HttpGet("search/product/{productId}")] // GET /api/orders/search/product/{productID}
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductId(Guid productId)
    {
        FilterDefinition<Order> filter = Builders<Order>
            .Filter.ElemMatch(x => x.OrderItems, Builders<OrderItem>.Filter.Eq(p => p.ProductID, productId));

        List<OrderResponse?> order = await _ordersService.GetOrdersByConditionAsync(filter);

        return order;
    }

    [HttpGet("search/orderDate/{orderDate}")] // GET /api/orders/search/orderDate/{orderDate}
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByOrderDate(DateTime orderDate)
    {
        FilterDefinition<Order> filter = Builders<Order>
            .Filter.Eq(x => x.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd"));

        List<OrderResponse?> order = await _ordersService.GetOrdersByConditionAsync(filter);

        return order;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductAsync(OrderAddRequest? orderAddRequest)
    {
        if (orderAddRequest is null)
        {
            return BadRequest("Invalid order data");
        }

        OrderResponse? orderResponse = await _ordersService.AddOrderAsync(orderAddRequest);

        if (orderResponse is null)
        {
            return Problem("Error in adding product");
        }

        return Created($"api/orders/search/orderid/{orderResponse}", orderResponse);
    }

    [HttpPut("{orderId}")]
    public async Task<IActionResult> UpdateOrderAsync(Guid orderId, OrderUpdateRequest? orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        if (orderId != orderUpdateRequest.OrderID)
        {
            return BadRequest("OrderID in the URL doesn't match with the OrderID in the Request body");
        }

        OrderResponse? orderResponse = await _ordersService.UpdateOrderAsync(orderUpdateRequest);

        if (orderResponse == null)
        {
            return Problem("Error in updating order");
        }
        
        return Ok(orderResponse);
    }

    [HttpDelete("{orderId}")]
    public async Task<IActionResult> DeleteProductAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest("Invalid order id");
        }

        bool isDeleted = await _ordersService.DeleteOrderAsync(orderId);

        if (!isDeleted)
        {
            return Problem("Error while deleting product");
        }

        return Ok(isDeleted);
    }


    [HttpGet("search/userid/{userId}")] // GET /api/orders/search/orderDate/{orderDate}
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByUserId(Guid userId)
    {
        FilterDefinition<Order> filter = Builders<Order>
            .Filter.Eq(x => x.UserID, userId);

        List<OrderResponse?> order = await _ordersService.GetOrdersByConditionAsync(filter);

        return order;
    }
}
