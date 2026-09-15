using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed record CreateEnergyOfferResult(
    Guid Id,
    OfferStatus Status,
    DateTimeOffset CreatedAt);