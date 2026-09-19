using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.EnergyOffers.GetById;

public sealed record GetEnergyOfferByIdResult(
    Guid Id,
    Guid SellerId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal PricePerMWh,
    Currency Currency,
    DateTimeOffset DeliveryStart,
    DateTimeOffset DeliveryEnd,
    OfferStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);