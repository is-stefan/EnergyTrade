using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Trades.GetById;

public sealed record GetTradeByIdResult(
    Guid Id,
    Guid EnergyOfferId,
    Guid SellerId,
    Guid BuyerId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal PricePerMWh,
    Currency Currency,
    DateTimeOffset CreatedAt);