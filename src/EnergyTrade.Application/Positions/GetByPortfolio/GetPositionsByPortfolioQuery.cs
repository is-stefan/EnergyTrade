using MediatR;

namespace EnergyTrade.Application.Positions.GetByPortfolio;

public sealed record GetPositionsByPortfolioQuery(
    Guid UserId,
    Guid PortfolioId)
    : IRequest<IReadOnlyList<GetPositionsByPortfolioResult>?>;
