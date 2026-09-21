namespace EnergyTrade.Application.Trades.GetAll;

public sealed record PagedTradesResult(
    IReadOnlyList<GetTradesResult> Items,
    int Page,
    int PageSize,
    int TotalCount);