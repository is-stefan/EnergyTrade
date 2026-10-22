using EnergyTrade.Application.Abstractions.Persistence;
using MediatR;

namespace EnergyTrade.Application.Portfolios.GetByUser;

public sealed class GetPortfoliosByUserQueryHandler
    : IRequestHandler<GetPortfoliosByUserQuery, IReadOnlyList<GetPortfoliosByUserResult>>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPortfoliosByUserQueryHandler(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<IReadOnlyList<GetPortfoliosByUserResult>> Handle(
        GetPortfoliosByUserQuery query,
        CancellationToken cancellationToken)
    {
        var portfolios = await _portfolioRepository.GetByUserIdAsync(
            query.UserId,
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
