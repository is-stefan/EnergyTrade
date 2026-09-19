using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.EnergyOffers.GetById;

public sealed class GetEnergyOfferByIdService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public GetEnergyOfferByIdService(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<GetEnergyOfferByIdResult?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var offer = await _energyOfferRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (offer is null)
        {
            return null;
        }

        return new GetEnergyOfferByIdResult(
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
            offer.UpdatedAt);
    }
}