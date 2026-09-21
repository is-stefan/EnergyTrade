using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface ITradeRepository
{
    Task AddAsync(
        Trade trade,
        CancellationToken cancellationToken = default);

    Task<Trade?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}