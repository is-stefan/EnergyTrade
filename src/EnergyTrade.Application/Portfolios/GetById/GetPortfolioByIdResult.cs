using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Portfolios.GetById;

public sealed record GetPortfolioByIdResult(
    Guid Id,
    Guid UserId,
    string Name,
    Currency BaseCurrency,
    PortfolioStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);