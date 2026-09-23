using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Orders.GetAll;

public sealed record GetOrdersResult(
    Guid Id,
    Guid BuyerId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal MaxPricePerMWh,
    Currency Currency,
    DateTimeOffset DeliveryStart,
    DateTimeOffset DeliveryEnd,
    OrderStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);