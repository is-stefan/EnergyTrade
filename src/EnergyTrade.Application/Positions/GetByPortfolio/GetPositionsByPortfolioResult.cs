using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Positions.GetByPortfolio;

public sealed record GetPositionsByPortfolioResult(
    Guid Id,
    Guid PortfolioId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);