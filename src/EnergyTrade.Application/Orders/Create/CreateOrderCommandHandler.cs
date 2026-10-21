using EnergyTrade.Application.Abstractions.Observability;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.Orders.Create;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IBusinessMetrics _businessMetrics;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IPortfolioRepository portfolioRepository,
        IBusinessMetrics businessMetrics)
    {
        _orderRepository = orderRepository;
        _portfolioRepository = portfolioRepository;
        _businessMetrics = businessMetrics;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (command.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(command.UserId));
        }

        var portfolio = await _portfolioRepository.GetByIdAsync(
            command.PortfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            throw new NotFoundException(
                "Portfolio does not exist.");
        }

        if (portfolio.UserId != command.UserId)
        {
            throw new ForbiddenException(
                "The portfolio does not belong to the authenticated user.");
        }

        if (portfolio.Status != PortfolioStatus.Active)
        {
            throw new ConflictException(
                "Only active portfolios can create orders.");
        }

        var order = new Order(
            command.UserId,
            command.PortfolioId,
            command.EnergyType,
            command.QuantityMWh,
            command.MaxPricePerMWh,
            command.Currency,
            command.DeliveryStart,
            command.DeliveryEnd);

        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        _businessMetrics.OrderCreated();

        return new CreateOrderResult(
            order.Id,
            order.Status,
            order.CreatedAt);
    }
}