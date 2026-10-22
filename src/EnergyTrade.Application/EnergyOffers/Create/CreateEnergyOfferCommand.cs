using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed record CreateEnergyOfferCommand(
    Guid UserId,
    Guid PortfolioId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal PricePerMWh,
    Currency Currency,
    DateTimeOffset DeliveryStart,
    DateTimeOffset DeliveryEnd)
    : IRequest<CreateEnergyOfferResult>
{
    public static CreateEnergyOfferCommand FromRequest(
        Guid userId,
        CreateEnergyOfferRequest request)
    {
        return new CreateEnergyOfferCommand(
            userId,
            request.PortfolioId,
            request.EnergyType,
            request.QuantityMWh,
            request.PricePerMWh,
            request.Currency,
            request.DeliveryStart,
            request.DeliveryEnd);
    }
}