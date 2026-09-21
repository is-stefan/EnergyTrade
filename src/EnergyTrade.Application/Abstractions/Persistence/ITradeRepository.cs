using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface ITradeRepository
{
    Task AddAsync(
        Trade trade,
        CancellationToken cancellationToken = default);

    Task<Trade?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Trade>> GetAllAsync(
        Guid? sellerId = null,
        Guid? buyerId = null,
        EnergyType? energyType = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid? sellerId = null,
        Guid? buyerId = null,
        EnergyType? energyType = null,
        CancellationToken cancellationToken = default);
}