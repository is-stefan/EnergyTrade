using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Tests.Entities;

public class PositionTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesZeroPosition()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();

        // Act
        var position = new Position(
            portfolioId,
            EnergyType.Solar);

        // Assert
        Assert.NotEqual(Guid.Empty, position.Id);
        Assert.Equal(portfolioId, position.PortfolioId);
        Assert.Equal(EnergyType.Solar, position.EnergyType);
        Assert.Equal(0m, position.QuantityMWh);
        Assert.NotEqual(default, position.CreatedAt);
        Assert.NotEqual(default, position.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyPortfolioId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Position(
                Guid.Empty,
                EnergyType.Solar));
    }

    [Fact]
    public void ApplyTrade_WithPositiveQuantity_IncreasesPosition()
    {
        // Arrange
        var position = CreatePosition();

        // Act
        position.ApplyTrade(100m);

        // Assert
        Assert.Equal(100m, position.QuantityMWh);
    }

    [Fact]
    public void ApplyTrade_WithNegativeQuantity_DecreasesPosition()
    {
        // Arrange
        var position = CreatePosition();

        // Act
        position.ApplyTrade(-100m);

        // Assert
        Assert.Equal(-100m, position.QuantityMWh);
    }

    [Fact]
    public void ApplyTrade_MultipleTimes_AccumulatesQuantity()
    {
        // Arrange
        var position = CreatePosition();

        // Act
        position.ApplyTrade(100m);
        position.ApplyTrade(50m);
        position.ApplyTrade(-30m);

        // Assert
        Assert.Equal(120m, position.QuantityMWh);
    }

    [Fact]
    public void ApplyTrade_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var position = CreatePosition();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => position.ApplyTrade(0m));
    }

    private static Position CreatePosition()
    {
        return new Position(
            Guid.NewGuid(),
            EnergyType.Solar);
    }
}