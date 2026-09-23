using EnergyTrade.Application.Abstractions.Persistence;

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

        order.Cancel();

        await _orderRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}