using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    public Guid BuyerId { get; private set; }

    public EnergyType EnergyType { get; private set; }

    public decimal QuantityMWh { get; private set; }

    public decimal MaxPricePerMWh { get; private set; }

    public Currency Currency { get; private set; }

    public DateTimeOffset DeliveryStart { get; private set; }

    public DateTimeOffset DeliveryEnd { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    private Order()
    {
    }

    public Order(
        Guid buyerId,
        EnergyType energyType,
        decimal quantityMWh,
        decimal maxPricePerMWh,
        Currency currency,
        DateTimeOffset deliveryStart,
        DateTimeOffset deliveryEnd)
    {
        if (buyerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Buyer id cannot be empty.",
                nameof(buyerId));
        }

        if (quantityMWh <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantityMWh),
                "Quantity must be greater than 0.");
        }

        if (maxPricePerMWh <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxPricePerMWh),
                "Maximum price must be greater than 0.");
        }

        if (deliveryEnd <= deliveryStart)
        {
            throw new ArgumentException(
                "Delivery end must be after delivery start.");
        }

        Id = Guid.NewGuid();
        BuyerId = buyerId;
        EnergyType = energyType;
        QuantityMWh = quantityMWh;
        MaxPricePerMWh = maxPricePerMWh;
        Currency = currency;
        DeliveryStart = deliveryStart;
        DeliveryEnd = deliveryEnd;
        Status = OrderStatus.Open;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Fill()
    {
        EnsureOpen();

        Status = OrderStatus.Filled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        EnsureOpen();

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureOpen()
    {
        if (Status != OrderStatus.Open)
        {
            throw new InvalidOperationException(
                "Only open orders can be modified.");
        }
    }
}