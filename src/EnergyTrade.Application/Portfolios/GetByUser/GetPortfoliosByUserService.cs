using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.Portfolios.GetByUser;

public sealed class GetPortfoliosByUserService
{
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPortfoliosByUserService(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<IReadOnlyList<GetPortfoliosByUserResult>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var portfolios = await _portfolioRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        return portfolios
            .Select(portfolio => new GetPortfoliosByUserResult(
                portfolio.Id,
                portfolio.UserId,
                portfolio.Name,
                portfolio.BaseCurrency,
                portfolio.Status,
                portfolio.CreatedAt,
                portfolio.UpdatedAt))
            .ToList();
    }
}