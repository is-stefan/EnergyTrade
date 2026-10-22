using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Orders.Cancel;

public sealed class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> Handle(
        CancelOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(
            command.OrderId,
            cancellationToken);

        if (order is null)
        {
            return false;
        }

        if (order.BuyerId != command.UserId)
        {
            throw new ForbiddenException(
                "The order does not belong to the authenticated user.");
        }

        order.Cancel();

        await _orderRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}