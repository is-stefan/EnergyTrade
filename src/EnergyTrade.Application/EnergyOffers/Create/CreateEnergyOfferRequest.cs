using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed record CreateEnergyOfferRequest(
    Guid SellerId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal PricePerMWh,
    Currency Currency,
    DateTimeOffset DeliveryStart,
    DateTimeOffset DeliveryEnd);
