using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Cancel;

public sealed record CancelEnergyOfferCommand(
    Guid UserId,
    Guid EnergyOfferId)
    : IRequest<bool>;