using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Close;

public sealed class CloseEnergyOfferCommandHandler
    : IRequestHandler<CloseEnergyOfferCommand, bool>
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public CloseEnergyOfferCommandHandler(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<bool> Handle(
        CloseEnergyOfferCommand command,
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

        offer.Close();

        await _energyOfferRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}