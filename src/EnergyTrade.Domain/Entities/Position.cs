using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Entities;

public class Position
{
    public Guid Id { get; private set; }

    public Guid PortfolioId { get; private set; }

    public EnergyType EnergyType { get; private set; }

    public decimal QuantityMWh { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private Position()
    {
    }

    public Position(
        Guid portfolioId,
        EnergyType energyType)
    {
        if (portfolioId == Guid.Empty)
        {
            throw new ArgumentException(
                "Portfolio id cannot be empty.",
                nameof(portfolioId));
        }

        Id = Guid.NewGuid();
        PortfolioId = portfolioId;
        EnergyType = energyType;
        QuantityMWh = 0m;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void ApplyTrade(decimal quantityDelta)
    {
        if (quantityDelta == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantityDelta),
                "Quantity delta cannot be zero.");
        }

        QuantityMWh += quantityDelta;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}