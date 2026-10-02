using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;

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
        Guid userId,
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

        if (offer.SellerId != userId)
        {
            throw new ForbiddenException(
                "The energy offer does not belong to the authenticated user.");
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