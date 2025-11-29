using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;

namespace BusinessLogicLayer.Services;

public sealed class OrderEnricher
{
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;
    private readonly UsersMicroServiceClient _usersMicroServiceClient;
    private readonly IMapper _mapper;

    public OrderEnricher(
        ProductsMicroserviceClient productsMicroserviceClient,
        IMapper mapper,
        UsersMicroServiceClient usersMicroServiceClient)
    {
        _productsMicroserviceClient = productsMicroserviceClient;
        _mapper = mapper;
        _usersMicroServiceClient = usersMicroServiceClient;
    }

    /// <summary>
    /// Enrich multiple orders with product details
    /// </summary>
    public async Task EnrichAsync(IEnumerable<OrderResponse?> orders)
    {
        foreach (var order in orders)
        {
            if (order is null)
                continue;

            await EnrichSingleOrderAsync(order);
        }
    }

    /// <summary>
    /// Enrich a single order with product details
    /// </summary>
    public Task EnrichAsync(OrderResponse order)
    {
        if (order is null)
            return Task.CompletedTask;

        return EnrichSingleOrderAsync(order);
    }

    private async Task EnrichSingleOrderAsync(OrderResponse order)
    {
        if (order.OrderItems is null)
            return;

        foreach (var item in order.OrderItems)
        {
            var productDto = await _productsMicroserviceClient.GetProductByProductIdAsync(item.ProductID);
            var userDto = await _usersMicroServiceClient.GetUserByIdAsync(order.UserID);

            if (productDto is null || userDto is null)
                continue;

            _mapper.Map(productDto, item);
            _mapper.Map(userDto, order);
        }
    }
}
