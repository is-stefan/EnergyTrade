using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Orders.Create;

public sealed record CreateOrderRequest(
    Guid BuyerId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal MaxPricePerMWh,
    Currency Currency,
    DateTimeOffset DeliveryStart,
    DateTimeOffset DeliveryEnd);