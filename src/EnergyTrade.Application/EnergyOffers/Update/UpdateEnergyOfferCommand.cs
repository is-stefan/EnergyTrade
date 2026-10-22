using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Update;

public sealed record UpdateEnergyOfferCommand(
    Guid UserId,
    Guid EnergyOfferId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal PricePerMWh,
    Currency Currency,
    DateTimeOffset DeliveryStart,
    DateTimeOffset DeliveryEnd)
    : IRequest<bool>
{
    public static UpdateEnergyOfferCommand FromRequest(
        Guid userId,
        Guid energyOfferId,
        UpdateEnergyOfferRequest request)
    {
        return new UpdateEnergyOfferCommand(
            userId,
            energyOfferId,
            request.EnergyType,
            request.QuantityMWh,
            request.PricePerMWh,
            request.Currency,
            request.DeliveryStart,
            request.DeliveryEnd);
    }
}