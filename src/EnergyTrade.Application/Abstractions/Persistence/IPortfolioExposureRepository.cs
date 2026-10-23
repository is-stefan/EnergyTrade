namespace EnergyTrade.Application.Abstractions.Persistence;

public interface IPortfolioExposureRepository
{
    Task<decimal> GetExposureAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);
}