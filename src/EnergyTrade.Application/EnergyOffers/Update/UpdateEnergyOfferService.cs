using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.EnergyOffers.Update;

public sealed class UpdateEnergyOfferService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public UpdateEnergyOfferService(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<bool> ExecuteAsync(
        Guid id,
        UpdateEnergyOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        var offer = await _energyOfferRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (offer is null)
        {
            return false;
        }

        offer.Update(
            request.EnergyType,
            request.QuantityMWh,
            request.PricePerMWh,
            request.Currency,
            request.DeliveryStart,
            request.DeliveryEnd);

        await _energyOfferRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}