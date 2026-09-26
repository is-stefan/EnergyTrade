using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Positions.ApplyTrade;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Application.Common.Exceptions;

namespace EnergyTrade.Application.Orders.Match;

public sealed class MatchOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEnergyOfferRepository _energyOfferRepository;
    private readonly ITradeRepository _tradeRepository;

    private readonly ApplyTradeToPositionsService _applyTradeToPositionsService;

    private readonly ITransactionManager _transactionManager;

    public MatchOrderService(
        IOrderRepository orderRepository,
        IEnergyOfferRepository energyOfferRepository,
        ITradeRepository tradeRepository,
        ApplyTradeToPositionsService applyTradeToPositionsService,
        ITransactionManager transactionManager)
    {
        _orderRepository = orderRepository;
        _energyOfferRepository = energyOfferRepository;
        _tradeRepository = tradeRepository;
        _applyTradeToPositionsService = applyTradeToPositionsService;
        _transactionManager = transactionManager;
    }

    public async Task<MatchOrderResult?> ExecuteAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        MatchOrderResult? result = null;

        try
        {
            await _transactionManager.ExecuteAsync(
                async transactionCancellationToken =>
                {
                    var order = await _orderRepository.GetByIdAsync(
                        orderId,
                        transactionCancellationToken);

                    if (order is null)
                    {
                        result = null;
                        return;
                    }

                    var offer = await _energyOfferRepository.FindMatchingAsync(
                        order.BuyerId,
                        order.EnergyType,
                        order.QuantityMWh,
                        order.MaxPricePerMWh,
                        order.Currency,
                        order.DeliveryStart,
                        order.DeliveryEnd,
                        transactionCancellationToken);

                    if (offer is null)
                    {
                        result = new MatchOrderResult(
                            false,
                            null,
                            null);

                        return;
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
                        transactionCancellationToken);

                    await _applyTradeToPositionsService.ExecuteAsync(
                        trade,
                        order.PortfolioId,
                        offer.PortfolioId,
                        transactionCancellationToken);

                    await _orderRepository.SaveChangesAsync(
                        transactionCancellationToken);

                    result = new MatchOrderResult(
                        true,
                        trade.Id,
                        offer.Id);
                },
                cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return new MatchOrderResult(
                false,
                null,
                null);
        }

        return result;
    }
}