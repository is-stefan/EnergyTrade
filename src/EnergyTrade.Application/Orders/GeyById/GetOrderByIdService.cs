using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.Orders.GetById;

public sealed class GetOrderByIdService
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdService(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<GetOrderByIdResult?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (order is null)
        {
            return null;
        }

        return new GetOrderByIdResult(
            order.Id,
            order.BuyerId,
            order.EnergyType,
            order.QuantityMWh,
            order.MaxPricePerMWh,
            order.Currency,
            order.DeliveryStart,
            order.DeliveryEnd,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt);
    }
}