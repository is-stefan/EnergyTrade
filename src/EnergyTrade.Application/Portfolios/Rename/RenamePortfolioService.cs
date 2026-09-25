using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.Portfolios.Rename;

public sealed class RenamePortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;

    public RenamePortfolioService(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<bool> ExecuteAsync(
        Guid portfolioId,
        RenamePortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(
            portfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            return false;
        }

        portfolio.Rename(request.Name);

        await _portfolioRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}