using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Portfolios.GetById;

public sealed class GetPortfolioByIdQueryHandler
    : IRequestHandler<GetPortfolioByIdQuery, GetPortfolioByIdResult?>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPortfolioByIdQueryHandler(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<GetPortfolioByIdResult?> Handle(
        GetPortfolioByIdQuery query,
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
