using MediatR;

namespace EnergyTrade.Application.EnergyOffers.GetById;

public sealed record GetEnergyOfferByIdQuery(
    Guid UserId,
    Guid OfferId)
    : IRequest<GetEnergyOfferByIdResult?>;