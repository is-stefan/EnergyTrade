using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Application.Common.Exceptions;

namespace EnergyTrade.Application.Orders.Create;

public sealed class CreateOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPortfolioRepository _portfolioRepository;

    public CreateOrderService(
        IOrderRepository orderRepository,
        IPortfolioRepository portfolioRepository)
    {
        _orderRepository = orderRepository;
        _portfolioRepository = portfolioRepository;
    }

    public async Task<CreateOrderResult> ExecuteAsync(
        Guid userId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(userId));
        }

        var portfolio = await _portfolioRepository.GetByIdAsync(
            request.PortfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            throw new ArgumentException(
                "Portfolio does not exist.",
                nameof(request.PortfolioId));
        }

        if (portfolio.UserId != userId)
        {
            throw new ForbiddenException(
                "The portfolio does not belong to the authenticated user.");
        }

        if (portfolio.Status != PortfolioStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active portfolios can create orders.");
        }

        var order = new Order(
            userId,
            request.PortfolioId,
            request.EnergyType,
            request.QuantityMWh,
            request.MaxPricePerMWh,
            request.Currency,
            request.DeliveryStart,
            request.DeliveryEnd);

        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        return new CreateOrderResult(
            order.Id,
            order.Status,
            order.CreatedAt);
    }
}