using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Portfolios.Exposure;

public sealed class GetPortfolioExposureQueryHandler
    : IRequestHandler<GetPortfolioExposureQuery, decimal>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IPortfolioExposureRepository _exposureRepository;

    public GetPortfolioExposureQueryHandler(
        IPortfolioRepository portfolioRepository,
        IPortfolioExposureRepository exposureRepository)
    {
        _portfolioRepository = portfolioRepository;
        _exposureRepository = exposureRepository;
    }

    public async Task<decimal> Handle(
        GetPortfolioExposureQuery request,
        CancellationToken cancellationToken)
    {
        var portfolio =
            await _portfolioRepository.GetByIdAsync(
                request.PortfolioId,
                cancellationToken);

        if (portfolio is null)
        {
            throw new NotFoundException(
                "Portfolio was not found.");
        }

        if (portfolio.UserId != request.UserId)
        {
            throw new ForbiddenException(
                "You are not allowed to access this portfolio.");
        }

        return await _exposureRepository.GetExposureAsync(
            request.PortfolioId,
            cancellationToken);
    }
}