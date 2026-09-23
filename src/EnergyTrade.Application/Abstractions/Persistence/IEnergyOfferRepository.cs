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
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        OfferStatus? status = null,
        EnergyType? energyType = null,
        CancellationToken cancellationToken = default);


    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<EnergyOffer?> FindMatchingAsync(
        Guid buyerId,
        EnergyType energyType,
        decimal quantityMWh,
        decimal maxPricePerMWh,
        Currency currency,
        DateTimeOffset deliveryStart,
        DateTimeOffset deliveryEnd,
        CancellationToken cancellationToken = default);
}