using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed class CreateEnergyOfferService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    private readonly IPortfolioRepository _portfolioRepository;

    public CreateEnergyOfferService(
        IEnergyOfferRepository energyOfferRepository,
        IPortfolioRepository portfolioRepository)
    {
        _energyOfferRepository = energyOfferRepository;
        _portfolioRepository = portfolioRepository;
    }

    public async Task<CreateEnergyOfferResult> ExecuteAsync(
        CreateEnergyOfferRequest request,
        CancellationToken cancellationToken = default)
    {

        var portfolio = await _portfolioRepository.GetByIdAsync(
            request.PortfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            throw new ArgumentException(
                "Portfolio does not exist.",
                nameof(request.PortfolioId));
        }

        if (portfolio.Status != PortfolioStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active portfolios can create energy offers.");
        }

        var energyOffer = new EnergyOffer(
            request.SellerId,
            request.PortfolioId,
            request.EnergyType,
            request.QuantityMWh,
            request.PricePerMWh,
            request.Currency,
            request.DeliveryStart,
            request.DeliveryEnd);

        await _energyOfferRepository.AddAsync(
            energyOffer,
            cancellationToken);

        return new CreateEnergyOfferResult(
            energyOffer.Id,
            energyOffer.Status,
            energyOffer.CreatedAt);
    }
}