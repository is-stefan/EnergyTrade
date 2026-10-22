using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Update;

public sealed class UpdateEnergyOfferCommandHandler
    : IRequestHandler<UpdateEnergyOfferCommand, bool>
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public UpdateEnergyOfferCommandHandler(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<bool> Handle(
        UpdateEnergyOfferCommand command,
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

        offer.Update(
            command.EnergyType,
            command.QuantityMWh,
            command.PricePerMWh,
            command.Currency,
            command.DeliveryStart,
            command.DeliveryEnd);

        await _energyOfferRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}