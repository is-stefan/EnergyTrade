using EnergyTrade.Application.Abstractions.Persistence;
using MediatR;

namespace EnergyTrade.Application.Orders.GetAll;

public sealed class GetOrdersQueryHandler
    : IRequestHandler<GetOrdersQuery, PagedOrdersResult>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedOrdersResult> Handle(
        GetOrdersQuery query,
        CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(query.UserId));
        }

        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.Page),
                "Page must be greater than 0.");
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.PageSize),
                "Page size must be between 1 and 100.");
        }

        var orders = await _orderRepository.GetAllAsync(
            query.UserId,
            query.EnergyType,
            query.Status,
            query.Page,
            query.PageSize,
            cancellationToken);

        var totalCount = await _orderRepository.CountAsync(
            query.UserId,
            query.EnergyType,
            query.Status,
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
            query.Page,
            query.PageSize,
            totalCount);
    }
}