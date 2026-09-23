using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetAllAsync(
        Guid? buyerId = null,
        EnergyType? energyType = null,
        OrderStatus? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid? buyerId = null,
        EnergyType? energyType = null,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default);
}