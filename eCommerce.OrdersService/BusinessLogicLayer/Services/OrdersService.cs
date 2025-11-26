using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Validators;
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


    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator)
    {
        _ordersRepository = ordersRepository;
        _mapper = mapper;
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
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

        // TODO: Add logic for checking if UserID exists in Users microservice

        Order orderInput = _mapper.Map<Order>(orderAddRequest);

        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(x => x.TotalPrice);

        Order? addedOrder = await _ordersRepository.AddOrderAsync(orderInput);

        return addedOrder is null ? null : _mapper.Map<OrderResponse>(addedOrder);
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

        return _mapper.Map<OrderResponse>(existingOrder);
    }

    public async Task<List<OrderResponse?>> GetOrdersAsync()
    {
        IEnumerable<Order?> existingOrders = await _ordersRepository.GetOrdersAsync();
        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(existingOrders);

        return orderResponses.ToList();
    }

    public async Task<List<OrderResponse?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> existingOrders = await _ordersRepository.GetOrdersByConditionAsync(filter);
        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(existingOrders);

        return orderResponses.ToList();
    }

    public async Task<OrderResponse?> UpdateOrderAsync(OrderUpdateRequest? orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }
        
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
        }


        Order orderInput = _mapper.Map<Order>( orderUpdateRequest); 
        
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

        return updatedOrderResponse;
    }
}