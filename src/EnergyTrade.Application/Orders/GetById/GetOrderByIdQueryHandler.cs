using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Orders.GetById;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, GetOrderByIdResult?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<GetOrderByIdResult?> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(
            query.Id,
            cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (order.BuyerId != query.UserId)
        {
            throw new ForbiddenException(
                "The order does not belong to the authenticated user.");
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