using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;

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
        Guid userId,
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

        if (offer.SellerId != userId)
        {
            throw new ForbiddenException(
                "The energy offer does not belong to the authenticated user.");
        }

        offer.Cancel();

        await _energyOfferRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}