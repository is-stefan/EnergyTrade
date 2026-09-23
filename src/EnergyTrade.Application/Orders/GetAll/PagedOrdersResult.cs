namespace EnergyTrade.Application.Orders.GetAll;

public sealed record PagedOrdersResult(
    IReadOnlyList<GetOrdersResult> Items,
    int Page,
    int PageSize,
    int TotalCount);