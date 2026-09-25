using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface IPositionRepository
{
    Task<EnergyTrade.Domain.Entities.Position?> GetByPortfolioAndEnergyTypeAsync(
        Guid portfolioId,
        EnergyType energyType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnergyTrade.Domain.Entities.Position>> GetByPortfolioIdAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        EnergyTrade.Domain.Entities.Position position,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}