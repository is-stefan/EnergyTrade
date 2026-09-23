using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Orders.GetAll;

public sealed class GetOrdersService
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersService(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedOrdersResult> ExecuteAsync(
        Guid? buyerId = null,
        EnergyType? energyType = null,
        OrderStatus? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100.");
        }

        var orders = await _orderRepository.GetAllAsync(
            buyerId,
            energyType,
            status,
            page,
            pageSize,
            cancellationToken);

        var totalCount = await _orderRepository.CountAsync(
            buyerId,
            energyType,
            status,
            cancellationToken);

        var items = orders
            .Select(order => new GetOrdersResult(
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
                order.UpdatedAt))
            .ToList();

        return new PagedOrdersResult(
            items,
            page,
            pageSize,
            totalCount);
    }
}