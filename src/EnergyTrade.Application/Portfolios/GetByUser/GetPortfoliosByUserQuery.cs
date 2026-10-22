using MediatR;

namespace EnergyTrade.Application.Portfolios.GetByUser;

public sealed record GetPortfoliosByUserQuery(
    Guid UserId)
    : IRequest<IReadOnlyList<GetPortfoliosByUserResult>>;
