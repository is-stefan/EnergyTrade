using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;

namespace EnergyTrade.Application.Orders.Cancel;

public sealed class CancelOrderService
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderService(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> ExecuteAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (order is null)
        {
            return false;
        }

        if (order.BuyerId != userId)
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