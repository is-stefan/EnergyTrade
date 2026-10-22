using MediatR;

namespace EnergyTrade.Application.Portfolios.GetById;

public sealed record GetPortfolioByIdQuery(
    Guid UserId,
    Guid PortfolioId)
    : IRequest<GetPortfolioByIdResult?>;
