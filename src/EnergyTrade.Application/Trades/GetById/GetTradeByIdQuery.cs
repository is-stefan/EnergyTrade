using MediatR;

namespace EnergyTrade.Application.Trades.GetById;

public sealed record GetTradeByIdQuery(
    Guid UserId,
    Guid TradeId)
    : IRequest<GetTradeByIdResult?>;
