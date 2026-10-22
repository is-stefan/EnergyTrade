using MediatR;

namespace EnergyTrade.Application.Orders.Cancel;

public sealed record CancelOrderCommand(
    Guid UserId,
    Guid OrderId)
    : IRequest<bool>;