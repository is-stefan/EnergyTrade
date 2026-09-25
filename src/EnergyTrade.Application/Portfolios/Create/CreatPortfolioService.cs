using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Portfolios.Create;

public sealed class CreatePortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;

    public CreatePortfolioService(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<CreatePortfolioResult> ExecuteAsync(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        var portfolio = new Portfolio(
            request.UserId,
            request.Name,
            request.BaseCurrency);

        await _portfolioRepository.AddAsync(
            portfolio,
            cancellationToken);

        await _portfolioRepository.SaveChangesAsync(
            cancellationToken);

        return new CreatePortfolioResult(
            portfolio.Id,
            portfolio.UserId,
            portfolio.Name,
            portfolio.BaseCurrency,
            portfolio.Status,
            portfolio.CreatedAt);
    }
}