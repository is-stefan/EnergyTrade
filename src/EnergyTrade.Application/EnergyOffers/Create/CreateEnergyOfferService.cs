using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed class CreateEnergyOfferService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;

    public CreateEnergyOfferService(
        IEnergyOfferRepository energyOfferRepository)
    {
        _energyOfferRepository = energyOfferRepository;
    }

    public async Task<CreateEnergyOfferResult> ExecuteAsync(
        CreateEnergyOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        var energyOffer = new EnergyOffer(
            request.SellerId,
            request.EnergyType,
            request.QuantityMWh,
            request.PricePerMWh,
            request.Currency,
            request.DeliveryStart,
            request.DeliveryEnd);

        await _energyOfferRepository.AddAsync(
            energyOffer,
            cancellationToken);

        return new CreateEnergyOfferResult(
            energyOffer.Id,
            energyOffer.Status,
            energyOffer.CreatedAt);
    }
}