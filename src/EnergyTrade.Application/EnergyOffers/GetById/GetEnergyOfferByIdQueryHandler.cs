using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.GetById;

public sealed class GetEnergyOfferByIdQueryHandler
    : IRequestHandler<GetEnergyOfferByIdQuery, GetEnergyOfferByIdResult?>
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public GetEnergyOfferByIdQueryHandler(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<GetEnergyOfferByIdResult?> Handle(
        GetEnergyOfferByIdQuery query,
        CancellationToken cancellationToken)
    {
        var offer = await _energyOfferRepository.GetByIdAsync(
            query.OfferId,
            cancellationToken);

        if (offer is null)
        {
            return null;
        }

        if (offer.SellerId != query.UserId)
        {
            throw new ForbiddenException(
                "The energy offer does not belong to the authenticated user.");
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