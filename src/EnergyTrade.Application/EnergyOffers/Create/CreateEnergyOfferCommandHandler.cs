using EnergyTrade.Application.Abstractions.Observability;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed class CreateEnergyOfferCommandHandler
    : IRequestHandler<CreateEnergyOfferCommand, CreateEnergyOfferResult>
{
    private readonly IEnergyOfferRepository _energyOfferRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IBusinessMetrics _businessMetrics;

    public CreateEnergyOfferCommandHandler(
        IEnergyOfferRepository energyOfferRepository,
        IPortfolioRepository portfolioRepository,
        IBusinessMetrics businessMetrics)
    {
        _energyOfferRepository = energyOfferRepository;
        _portfolioRepository = portfolioRepository;
        _businessMetrics = businessMetrics;
    }

    public async Task<CreateEnergyOfferResult> Handle(
        CreateEnergyOfferCommand command,
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
                "Only active portfolios can create energy offers.");
        }

        var energyOffer = new EnergyOffer(
            command.UserId,
            command.PortfolioId,
            command.EnergyType,
            command.QuantityMWh,
            command.PricePerMWh,
            command.Currency,
            command.DeliveryStart,
            command.DeliveryEnd);

        await _energyOfferRepository.AddAsync(
            energyOffer,
            cancellationToken);

        _businessMetrics.EnergyOfferCreated();

        return new CreateEnergyOfferResult(
            energyOffer.Id,
            energyOffer.Status,
            energyOffer.CreatedAt);
    }
}