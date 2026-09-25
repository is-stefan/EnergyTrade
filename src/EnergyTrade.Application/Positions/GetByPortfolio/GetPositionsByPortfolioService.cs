using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.Positions.GetByPortfolio;

public sealed class GetPositionsByPortfolioService
{
    private readonly IPositionRepository _positionRepository;

    public GetPositionsByPortfolioService(
        IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<IReadOnlyList<GetPositionsByPortfolioResult>> ExecuteAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
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