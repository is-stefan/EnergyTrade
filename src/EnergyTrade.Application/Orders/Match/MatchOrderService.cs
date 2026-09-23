using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Orders.Match;

public sealed class MatchOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEnergyOfferRepository _energyOfferRepository;
    private readonly ITradeRepository _tradeRepository;

    public MatchOrderService(
        IOrderRepository orderRepository,
        IEnergyOfferRepository energyOfferRepository,
        ITradeRepository tradeRepository)
    {
        _orderRepository = orderRepository;
        _energyOfferRepository = energyOfferRepository;
        _tradeRepository = tradeRepository;
    }

    public async Task<MatchOrderResult?> ExecuteAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(
            orderId,
            cancellationToken);

        if (order is null)
        {
            return null;
        }

        var offer = await _energyOfferRepository.FindMatchingAsync(
            order.BuyerId,
            order.EnergyType,
            order.QuantityMWh,
            order.MaxPricePerMWh,
            order.Currency,
            order.DeliveryStart,
            order.DeliveryEnd,
            cancellationToken);

        if (offer is null)
        {
            return new MatchOrderResult(
                false,
                null,
                null);
        }

        var trade = new Trade(
            offer.Id,
            offer.SellerId,
            order.BuyerId,
            offer.EnergyType,
            order.QuantityMWh,
            offer.PricePerMWh,
            offer.Currency);

        order.Fill();
        offer.Close();

        await _tradeRepository.AddAsync(
            trade,
            cancellationToken);

        await _orderRepository.SaveChangesAsync(
            cancellationToken);

        return new MatchOrderResult(
            true,
            trade.Id,
            offer.Id);
    }
}