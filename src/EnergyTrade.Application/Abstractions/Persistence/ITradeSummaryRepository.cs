using EnergyTrade.Application.Trades.Summary;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface ITradeSummaryRepository
{
    Task<TradeSummaryResult> GetUserTradeSummaryAsync(
        Guid userId,
        Currency currency,
        EnergyType? energyType = null,
        CancellationToken cancellationToken = default);
}