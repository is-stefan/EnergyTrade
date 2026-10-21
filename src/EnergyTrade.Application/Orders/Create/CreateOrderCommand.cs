using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.Orders.Create;

public sealed record CreateOrderCommand(
    Guid UserId,
    Guid PortfolioId,
    EnergyType EnergyType,
    decimal QuantityMWh,
    decimal MaxPricePerMWh,
    Currency Currency,
    DateTimeOffset DeliveryStart,
    DateTimeOffset DeliveryEnd)
    : IRequest<CreateOrderResult>
{
    public static CreateOrderCommand FromRequest(Guid userId, CreateOrderRequest request) =>
        new(userId, request.PortfolioId, request.EnergyType, request.QuantityMWh,
            request.MaxPricePerMWh, request.Currency, request.DeliveryStart, request.DeliveryEnd);
}