using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.Trades.Summary;

public sealed record GetTradeSummaryQuery(
    Guid UserId,
    Currency Currency,
    EnergyType? EnergyType = null)
    : IRequest<TradeSummaryResult>;