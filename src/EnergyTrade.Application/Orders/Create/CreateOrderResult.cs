using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Orders.Create;

public sealed record CreateOrderResult(
    Guid Id,
    OrderStatus Status,
    DateTimeOffset CreatedAt);