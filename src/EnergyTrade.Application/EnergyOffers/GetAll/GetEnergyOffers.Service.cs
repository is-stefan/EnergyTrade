using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.EnergyOffers.GetAll;

public sealed class GetEnergyOffersService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public GetEnergyOffersService(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<IReadOnlyList<GetEnergyOffersResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var offers = await _energyOfferRepository.GetAllAsync(
            cancellationToken);

        return offers
            .Select(offer => new GetEnergyOffersResult(
                offer.Id,
                offer.SellerId,
                offer.EnergyType,
                offer.QuantityMWh,
                offer.PricePerMWh,
                offer.Currency,
                offer.DeliveryStart,
                offer.DeliveryEnd,
                offer.Status,
                offer.CreatedAt,
                offer.UpdatedAt))
            .ToList();
    }
}