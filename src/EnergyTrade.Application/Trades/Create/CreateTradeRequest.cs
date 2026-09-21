namespace EnergyTrade.Application.Trades.Create;

public sealed record CreateTradeRequest(
    Guid EnergyOfferId,
    Guid BuyerId);