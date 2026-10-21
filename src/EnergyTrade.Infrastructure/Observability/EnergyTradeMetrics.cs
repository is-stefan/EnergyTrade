using System.Diagnostics.Metrics;
using EnergyTrade.Application.Abstractions.Observability;

namespace EnergyTrade.Infrastructure.Observability;

public sealed class EnergyTradeMetrics : IBusinessMetrics
{
    public const string MeterName = "EnergyTrade.Business";

    private static readonly Meter Meter = new(MeterName);

    private readonly Counter<long> _ordersCreated;
    private readonly Counter<long> _energyOffersCreated;
    private readonly Counter<long> _tradesCompleted;

    private readonly Histogram<double> _orderMatchingDuration;

    public EnergyTradeMetrics()
    {
        _ordersCreated = Meter.CreateCounter<long>(
            "energytrade.orders.created");

        _energyOffersCreated = Meter.CreateCounter<long>(
            "energytrade.offers.created");

        _tradesCompleted = Meter.CreateCounter<long>(
            "energytrade.trades.completed");

        _orderMatchingDuration = Meter.CreateHistogram<double>(
            "energytrade.order_matching.duration",
            unit: "ms");
    }

    public void OrderCreated()
    {
        _ordersCreated.Add(1);
    }

    public void EnergyOfferCreated()
    {
        _energyOffersCreated.Add(1);
    }

    public void TradeCompleted()
    {
        _tradesCompleted.Add(1);
    }

    public void RecordOrderMatchingDuration(double milliseconds)
    {
        _orderMatchingDuration.Record(milliseconds);
    }
}