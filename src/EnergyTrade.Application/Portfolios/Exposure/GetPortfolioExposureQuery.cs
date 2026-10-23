using MediatR;

namespace EnergyTrade.Application.Portfolios.Exposure;

public sealed record GetPortfolioExposureQuery(
    Guid UserId,
    Guid PortfolioId)
    : IRequest<decimal>;