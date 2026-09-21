using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Trades.GetAll;

public sealed record GetTradesResult(
    Guid Id,
    Guid EnergyOfferId,
    Guid SellerId,
    Guid BuyerId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal PricePerMWh,
    Currency Currency,
    DateTimeOffset CreatedAt);