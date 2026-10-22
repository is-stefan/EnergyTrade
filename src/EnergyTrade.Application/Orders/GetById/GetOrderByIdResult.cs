using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Orders.GetById;

public sealed record GetOrderByIdResult(
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