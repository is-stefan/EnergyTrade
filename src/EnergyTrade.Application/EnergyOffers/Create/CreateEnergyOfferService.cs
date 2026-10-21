using EnergyTrade.Application.Abstractions.Observability;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed class CreateEnergyOfferService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IBusinessMetrics _businessMetrics;

    public CreateEnergyOfferService(
        IEnergyOfferRepository energyOfferRepository,
        IPortfolioRepository portfolioRepository,
        IBusinessMetrics businessMetrics)
    {
        _energyOfferRepository = energyOfferRepository;
        _portfolioRepository = portfolioRepository;
        _businessMetrics = businessMetrics;
    }

    public async Task<CreateEnergyOfferResult> ExecuteAsync(
        Guid userId,
        CreateEnergyOfferRequest request,
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
            throw new NotFoundException(
                "Portfolio does not exist.");
        }

        if (portfolio.UserId != userId)
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
            userId,
            request.PortfolioId,
            request.EnergyType,
            request.QuantityMWh,
            request.PricePerMWh,
            request.Currency,
            request.DeliveryStart,
            request.DeliveryEnd);

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