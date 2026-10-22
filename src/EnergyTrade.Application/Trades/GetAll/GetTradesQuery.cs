using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.Trades.GetAll;

public sealed record GetTradesQuery(
    Guid UserId,
    EnergyType? EnergyType = null,
    int Page = 1,
    int PageSize = 10)
    : IRequest<PagedTradesResult>;
