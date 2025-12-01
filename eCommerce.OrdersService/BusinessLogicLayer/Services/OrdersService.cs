using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services;

public sealed class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly UsersMicroServiceClient _usersMicroServiceClient;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;
    private readonly OrderEnricher _orderProductEnricher;

    public OrdersService(
        IOrdersRepository ordersRepository,
        IMapper mapper,
        IValidator<OrderAddRequest> orderAddRequestValidator,
        IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
        IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
        IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
        UsersMicroServiceClient usersMicroServiceClient,
        ProductsMicroserviceClient productsMicroserviceClient,
        OrderEnricher orderProductEnricher)
    {
        _ordersRepository = ordersRepository;
        _mapper = mapper;
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _usersMicroServiceClient = usersMicroServiceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
        _orderProductEnricher = orderProductEnricher;
    }

    public async Task<OrderResponse?> AddOrderAsync(OrderAddRequest? orderAddRequest)
    {
        ArgumentNullException.ThrowIfNull(orderAddRequest);
        ValidationResult validationResult = _orderAddRequestValidator.Validate(orderAddRequest);

        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage));
            throw new ValidationException(errors);
        }

        foreach (var orderItemAddRequest in orderAddRequest.OrderItems)
        {
            var orderItemValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);
            if (!orderItemValidationResult.IsValid)
            {
                string errors = string.Join(", ",
                    orderItemValidationResult.Errors.Select(x => x.ErrorMessage));
                throw new ValidationException(errors);
            }

            _ = await _productsMicroserviceClient
                    .GetProductByProductIdAsync(orderItemAddRequest.ProductID) ??
                throw new ArgumentException($"Product with ID {orderItemAddRequest.ProductID} does not exist.");
        }

        UserDto? userDto = await _usersMicroServiceClient.GetUserByIdAsync(orderAddRequest.UserID);
        if (userDto is null)
        {
            throw new ArgumentException($"User with ID {orderAddRequest.UserID} does not exist.");
        }

        Order orderInput = _mapper.Map<Order>(orderAddRequest);

        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(x => x.TotalPrice);

        Order? addedOrder = await _ordersRepository.AddOrderAsync(orderInput);

        if (addedOrder is null)
            return null;

        OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);
        await _orderProductEnricher.EnrichAsync(addedOrderResponse);

        return addedOrderResponse;
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderID, orderId);
        Order? existingOrder = await _ordersRepository.GetOrderByConditionAsync(filter);

        if (existingOrder is null) return false;

        return await _ordersRepository.DeleteOrderAsync(orderId);
    }

    public async Task<OrderResponse?> GetOrderByConditionAsync(FilterDefinition<Order> filter)
    {
        Order? existingOrder = await _ordersRepository.GetOrderByConditionAsync(filter);

        if (existingOrder is null) return null;

        OrderResponse orderResponse = _mapper.Map<OrderResponse>(existingOrder);
        await _orderProductEnricher.EnrichAsync(orderResponse);

        return orderResponse;
    }

    public async Task<List<OrderResponse?>> GetOrdersAsync()
    {
        IEnumerable<Order?> existingOrders = await _ordersRepository.GetOrdersAsync();
        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(existingOrders);

        await _orderProductEnricher.EnrichAsync(orderResponses);

        return orderResponses.ToList();
    }

    public async Task<List<OrderResponse?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> existingOrders = await _ordersRepository.GetOrdersByConditionAsync(filter);
        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(existingOrders);

        await _orderProductEnricher.EnrichAsync(orderResponses);

        return orderResponses.ToList();
    }

    public async Task<OrderResponse?> UpdateOrderAsync(OrderUpdateRequest? orderUpdateRequest)
    {
        ArgumentNullException.ThrowIfNull(orderUpdateRequest);

        ValidationResult orderUpdateRequestValidationResult =
            await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);

        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ",
                orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult =
                await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ",
                    orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }

            _ = await _productsMicroserviceClient
                    .GetProductByProductIdAsync(orderItemUpdateRequest.ProductID) ??
                throw new ArgumentException($"Product with ID {orderItemUpdateRequest.ProductID} does not exist.");
        }

        UserDto? userDto = await _usersMicroServiceClient.GetUserByIdAsync(orderUpdateRequest.UserID);
        if (userDto is null)
        {
            throw new ArgumentException($"User with ID {orderUpdateRequest.UserID} does not exist.");
        }

        Order orderInput = _mapper.Map<Order>(orderUpdateRequest);

        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

        Order? updatedOrder = await _ordersRepository.UpdateOrderAsync(orderInput);

        if (updatedOrder == null)
        {
            return null;
        }

        OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder);

        await _orderProductEnricher.EnrichAsync(updatedOrderResponse);

        return updatedOrderResponse;
    }
}