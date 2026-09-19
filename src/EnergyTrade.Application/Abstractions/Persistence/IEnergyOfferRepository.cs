using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface IEnergyOfferRepository
{
    Task AddAsync(
        EnergyOffer energyOffer,
        CancellationToken cancellationToken = default);

    Task<EnergyOffer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}