using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Cancel;

public sealed class CancelEnergyOfferCommandHandler
    : IRequestHandler<CancelEnergyOfferCommand, bool>
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public CancelEnergyOfferCommandHandler(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<bool> Handle(
        CancelEnergyOfferCommand command,
        CancellationToken cancellationToken)
    {
        var offer = await _energyOfferRepository.GetByIdAsync(
            command.EnergyOfferId,
            cancellationToken);

        if (offer is null)
        {
            return false;
        }

        if (offer.SellerId != command.UserId)
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