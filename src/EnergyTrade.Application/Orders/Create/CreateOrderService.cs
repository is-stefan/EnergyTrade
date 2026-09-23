using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Orders.Create;

public sealed class CreateOrderService
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderService(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<CreateOrderResult> ExecuteAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = new Order(
            request.BuyerId,
            request.EnergyType,
            request.QuantityMWh,
            request.MaxPricePerMWh,
            request.Currency,
            request.DeliveryStart,
            request.DeliveryEnd);

        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        return new CreateOrderResult(
            order.Id,
            order.Status,
            order.CreatedAt);
    }
}