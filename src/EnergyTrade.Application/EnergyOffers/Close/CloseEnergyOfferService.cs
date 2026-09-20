using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.EnergyOffers.Close;

public sealed class CloseEnergyOfferService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public CloseEnergyOfferService(
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

        offer.Close();

        await _energyOfferRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}