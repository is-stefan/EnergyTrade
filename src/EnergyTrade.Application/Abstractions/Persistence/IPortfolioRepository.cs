using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface IPortfolioRepository
{
    Task AddAsync(
        Portfolio portfolio,
        CancellationToken cancellationToken = default);

    Task<Portfolio?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}