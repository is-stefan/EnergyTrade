using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Portfolios.Create;

public sealed record CreatePortfolioResult(
    Guid Id,
    Guid UserId,
    string Name,
    Currency BaseCurrency,
    PortfolioStatus Status,
    DateTimeOffset CreatedAt);