using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Entities;

public class Trade
{
    public Guid Id { get; private set; }

    public Guid EnergyOfferId { get; private set; }

    public Guid SellerId { get; private set; }

    public Guid BuyerId { get; private set; }

    public EnergyType EnergyType { get; private set; }

    public decimal QuantityMWh { get; private set; }

    public decimal PricePerMWh { get; private set; }

    public Currency Currency { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private Trade()
    {
    }

    public Trade(
        Guid energyOfferId,
        Guid sellerId,
        Guid buyerId,
        EnergyType energyType,
        decimal quantityMWh,
        decimal pricePerMWh,
        Currency currency)
    {
        if (energyOfferId == Guid.Empty)
        {
            throw new ArgumentException(
                "Energy offer id cannot be empty.",
                nameof(energyOfferId));
        }

        if (sellerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Seller id cannot be empty.",
                nameof(sellerId));
        }

        if (buyerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Buyer id cannot be empty.",
                nameof(buyerId));
        }

        if (sellerId == buyerId)
        {
            throw new InvalidOperationException(
                "Seller and buyer cannot be the same user.");
        }

        if (quantityMWh <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantityMWh),
                "Quantity must be greater than 0.");
        }

        if (pricePerMWh <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pricePerMWh),
                "Price must be greater than 0.");
        }

        Id = Guid.NewGuid();
        EnergyOfferId = energyOfferId;
        SellerId = sellerId;
        BuyerId = buyerId;
        EnergyType = energyType;
        QuantityMWh = quantityMWh;
        PricePerMWh = pricePerMWh;
        Currency = currency;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}