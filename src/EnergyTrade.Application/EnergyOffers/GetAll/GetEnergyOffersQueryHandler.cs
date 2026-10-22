using EnergyTrade.Application.Abstractions.Persistence;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.GetAll;

public sealed class GetEnergyOffersQueryHandler
    : IRequestHandler<GetEnergyOffersQuery, PagedEnergyOffersResult>
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public GetEnergyOffersQueryHandler(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<PagedEnergyOffersResult> Handle(
        GetEnergyOffersQuery query,
        CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(query.UserId));
        }

        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.Page),
                "Page must be greater than 0.");
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.PageSize),
                "Page size must be between 1 and 100.");
        }

        var offers = await _energyOfferRepository.GetAllAsync(
            query.UserId,
            query.Status,
            query.EnergyType,
            query.Page,
            query.PageSize,
            cancellationToken);

        var totalCount = await _energyOfferRepository.CountAsync(
            query.UserId,
            query.Status,
            query.EnergyType,
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
            query.Page,
            query.PageSize,
            totalCount);
    }
}