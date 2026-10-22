using MediatR;

namespace EnergyTrade.Application.Orders.Match;

public sealed record MatchOrderCommand(
    Guid UserId,
    Guid OrderId)
    : IRequest<MatchOrderResult?>;