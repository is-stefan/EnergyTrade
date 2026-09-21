using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Trades.Create;

public sealed class CreateTradeService
{
    private readonly IEnergyOfferRepository _energyOfferRepository;
    private readonly ITradeRepository _tradeRepository;

    public CreateTradeService(
        IEnergyOfferRepository energyOfferRepository,
        ITradeRepository tradeRepository)
    {
        _energyOfferRepository = energyOfferRepository;
        _tradeRepository = tradeRepository;
    }

    public async Task<CreateTradeResult?> ExecuteAsync(
        CreateTradeRequest request,
        CancellationToken cancellationToken = default)
    {
        var offer = await _energyOfferRepository.GetByIdAsync(
            request.EnergyOfferId,
            cancellationToken);

        if (offer is null)
        {
            return null;
        }

        var trade = new Trade(
            offer.Id,
            offer.SellerId,
            request.BuyerId,
            offer.EnergyType,
            offer.QuantityMWh,
            offer.PricePerMWh,
            offer.Currency);

        offer.Close();

        await _tradeRepository.AddAsync(
            trade,
            cancellationToken);

        await _energyOfferRepository.SaveChangesAsync(
            cancellationToken);

        return new CreateTradeResult(
            trade.Id,
            trade.EnergyOfferId,
            trade.SellerId,
            trade.BuyerId,
            trade.EnergyType,
            trade.QuantityMWh,
            trade.PricePerMWh,
            trade.Currency,
            trade.CreatedAt);
    }
}