namespace EnergyTrade.Application.EnergyOffers.GetAll;

public sealed record PagedEnergyOffersResult(
    IReadOnlyList<GetEnergyOffersResult> Items,
    int Page,
    int PageSize,
    int TotalCount);