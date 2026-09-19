using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface IEnergyOfferRepository
{
    Task AddAsync(
        EnergyOffer energyOffer,
        CancellationToken cancellationToken = default);

    Task<EnergyOffer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnergyOffer>> GetAllAsync(
        OfferStatus? status = null,
        EnergyType? energyType = null,
        CancellationToken cancellationToken = default);
}