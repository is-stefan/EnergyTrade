namespace EnergyTrade.Application.Trades.Summary;

public sealed record TradeSummaryResult(
    int TradeCount,
    decimal TotalMWh,
    decimal AveragePrice,
    decimal TotalValue);