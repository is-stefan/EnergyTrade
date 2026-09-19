using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.EnergyOffers.GetAll;

public sealed record GetEnergyOffersResult(
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