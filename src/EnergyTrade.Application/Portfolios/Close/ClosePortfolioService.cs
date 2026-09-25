using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.Portfolios.Close;

public sealed class ClosePortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;

    public ClosePortfolioService(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<bool> ExecuteAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(
            portfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            return false;
        }

        portfolio.Close();

        await _portfolioRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}