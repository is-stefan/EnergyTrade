using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Positions.GetByPortfolio;

public sealed class GetPositionsByPortfolioQueryHandler
    : IRequestHandler<
        GetPositionsByPortfolioQuery,
        IReadOnlyList<GetPositionsByPortfolioResult>?>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPositionsByPortfolioQueryHandler(
        IPositionRepository positionRepository,
        IPortfolioRepository portfolioRepository)
    {
        _positionRepository = positionRepository;
        _portfolioRepository = portfolioRepository;
    }

    public async Task<IReadOnlyList<GetPositionsByPortfolioResult>?> Handle(
        GetPositionsByPortfolioQuery query,
        CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(
            query.PortfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            return null;
        }

        if (portfolio.UserId != query.UserId)
        {
            throw new ForbiddenException(
                "The portfolio does not belong to the authenticated user.");
        }

        var positions =
            await _positionRepository.GetByPortfolioIdAsync(
                query.PortfolioId,
                cancellationToken);

        return positions
            .Select(position => new GetPositionsByPortfolioResult(
                position.Id,
                position.PortfolioId,
                position.EnergyType,
                position.QuantityMWh,
                position.CreatedAt,
                position.UpdatedAt))
            .ToList();
    }
}
