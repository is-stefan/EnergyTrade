using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.EnergyOffers.Cancel;

public sealed class CancelEnergyOfferService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public CancelEnergyOfferService(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<bool> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var offer = await _energyOfferRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (offer is null)
        {
            return false;
        }

        offer.Cancel();

        await _energyOfferRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}