using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;

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
        Guid userId,
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

        if (portfolio.UserId != userId)
        {
            throw new ForbiddenException(
                "The portfolio does not belong to the authenticated user.");
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