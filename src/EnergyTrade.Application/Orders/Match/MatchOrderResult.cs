namespace EnergyTrade.Application.Orders.Match;

public sealed record MatchOrderResult(
    bool Matched,
    Guid? TradeId,
    Guid? EnergyOfferId);