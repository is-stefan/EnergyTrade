using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;

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
        Guid userId,
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

        if (portfolio.UserId != userId)
        {
            throw new ForbiddenException(
                "The portfolio does not belong to the authenticated user.");
        }

        portfolio.Close();

        await _portfolioRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}