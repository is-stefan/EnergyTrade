using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Portfolios.GetByUser;

public sealed record GetPortfoliosByUserResult(
    Guid Id,
    Guid UserId,
    string Name,
    Currency BaseCurrency,
    PortfolioStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);