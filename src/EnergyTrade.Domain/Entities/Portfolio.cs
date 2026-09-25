using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Entities;

public class Portfolio
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public Currency BaseCurrency { get; private set; }

    public PortfolioStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    private Portfolio()
    {
        Name = string.Empty;
    }

    public Portfolio(
        Guid userId,
        string name,
        Currency baseCurrency)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Portfolio name cannot be empty.",
                nameof(name));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name.Trim();
        BaseCurrency = baseCurrency;
        Status = PortfolioStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Rename(string name)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Portfolio name cannot be empty.",
                nameof(name));
        }

        Name = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Close()
    {
        EnsureActive();

        Status = PortfolioStatus.Closed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != PortfolioStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active portfolios can be modified.");
        }
    }
}