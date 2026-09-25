using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Portfolios.Create;

public sealed record CreatePortfolioRequest(
    Guid UserId,
    string Name,
    Currency BaseCurrency);