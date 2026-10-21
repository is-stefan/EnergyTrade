namespace EnergyTrade.Application.Abstractions.Observability;

public interface IBusinessMetrics
{
    void OrderCreated();
    void EnergyOfferCreated();
    void TradeCompleted();

    void RecordOrderMatchingDuration(double milliseconds);
}