using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.Orders.GetAll;

public sealed record GetOrdersQuery(
    Guid UserId,
    EnergyType? EnergyType = null,
    OrderStatus? Status = null,
    int Page = 1,
    int PageSize = 10)
    : IRequest<PagedOrdersResult>;