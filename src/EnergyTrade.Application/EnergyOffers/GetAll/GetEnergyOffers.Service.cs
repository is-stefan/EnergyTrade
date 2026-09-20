using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.EnergyOffers.GetAll;

public sealed class GetEnergyOffersService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public GetEnergyOffersService(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<PagedEnergyOffersResult> ExecuteAsync(
        OfferStatus? status = null,
        EnergyType? energyType = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {

        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100.");
        }

        var offers = await _energyOfferRepository.GetAllAsync(
            status,
            energyType,
            page,
            pageSize,
            cancellationToken);

        var totalCount = await _energyOfferRepository.CountAsync(
            status,
            energyType,
            cancellationToken);

        var items = offers
            .Select(offer => new GetEnergyOffersResult(
                offer.Id,
                offer.SellerId,
                offer.EnergyType,
                offer.QuantityMWh,
                offer.PricePerMWh,
                offer.Currency,
                offer.DeliveryStart,
                offer.DeliveryEnd,
                offer.Status,
                offer.CreatedAt,
                offer.UpdatedAt))
            .ToList();

        return new PagedEnergyOffersResult(
            items,
            page,
            pageSize,
            totalCount);
    }
}