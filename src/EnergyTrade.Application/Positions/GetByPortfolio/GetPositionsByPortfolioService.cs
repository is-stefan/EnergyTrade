using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;

namespace EnergyTrade.Application.Positions.GetByPortfolio;

public sealed class GetPositionsByPortfolioService
{
    private readonly IPositionRepository _positionRepository;
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPositionsByPortfolioService(
        IPositionRepository positionRepository,
        IPortfolioRepository portfolioRepository)
    {
        _positionRepository = positionRepository;
        _portfolioRepository = portfolioRepository;
    }

    public async Task<IReadOnlyList<GetPositionsByPortfolioResult>?> ExecuteAsync(
        Guid userId,
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(
            portfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            return null;
        }

        if (portfolio.UserId != userId)
        {
            throw new ForbiddenException(
                "The portfolio does not belong to the authenticated user.");
        }

        var positions =
            await _positionRepository.GetByPortfolioIdAsync(
                portfolioId,
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