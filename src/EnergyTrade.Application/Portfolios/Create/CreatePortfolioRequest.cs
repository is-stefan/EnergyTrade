using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Portfolios.Create;

public sealed record CreatePortfolioRequest(
    string Name,
    Currency BaseCurrency);