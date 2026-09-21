using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Trades.Create;

public sealed record CreateTradeResult(
    Guid Id,
    Guid EnergyOfferId,
    Guid SellerId,
    Guid BuyerId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal PricePerMWh,
    Currency Currency,
    DateTimeOffset CreatedAt);