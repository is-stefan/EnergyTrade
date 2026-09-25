using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.Portfolios.GetById;

public sealed class GetPortfolioByIdService
{
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPortfolioByIdService(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<GetPortfolioByIdResult?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (portfolio is null)
        {
            return null;
        }

        return new GetPortfolioByIdResult(
            portfolio.Id,
            portfolio.UserId,
            portfolio.Name,
            portfolio.BaseCurrency,
            portfolio.Status,
            portfolio.CreatedAt,
            portfolio.UpdatedAt);
    }
}