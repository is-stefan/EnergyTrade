using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.EnergyOffers.Update;

public sealed record UpdateEnergyOfferRequest(
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal PricePerMWh,
    Currency Currency,
    DateTimeOffset DeliveryStart,
    DateTimeOffset DeliveryEnd);