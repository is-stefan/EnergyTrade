using MediatR;

namespace EnergyTrade.Application.Orders.GetById;

public sealed record GetOrderByIdQuery(
    Guid UserId,
    Guid Id)
    : IRequest<GetOrderByIdResult?>;