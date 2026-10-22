using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Close;

public sealed record CloseEnergyOfferCommand(
    Guid UserId,
    Guid EnergyOfferId)
    : IRequest<bool>;