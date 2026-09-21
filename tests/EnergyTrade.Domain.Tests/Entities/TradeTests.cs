using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Tests.Entities;

public class TradeTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesTrade()
    {
        // Arrange
        var energyOfferId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();

        // Act
        var trade = new Trade(
            energyOfferId,
            sellerId,
            buyerId,
            EnergyType.Solar,
            100m,
            85m,
            Currency.EUR);

        // Assert
        Assert.NotEqual(Guid.Empty, trade.Id);
        Assert.Equal(energyOfferId, trade.EnergyOfferId);
        Assert.Equal(sellerId, trade.SellerId);
        Assert.Equal(buyerId, trade.BuyerId);
        Assert.Equal(EnergyType.Solar, trade.EnergyType);
        Assert.Equal(100m, trade.QuantityMWh);
        Assert.Equal(85m, trade.PricePerMWh);
        Assert.Equal(Currency.EUR, trade.Currency);
        Assert.NotEqual(default, trade.CreatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyEnergyOfferId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Trade(
                Guid.Empty,
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                100m,
                85m,
                Currency.EUR));
    }

    [Fact]
    public void Constructor_WithEmptySellerId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Trade(
                Guid.NewGuid(),
                Guid.Empty,
                Guid.NewGuid(),
                EnergyType.Solar,
                100m,
                85m,
                Currency.EUR));
    }

    [Fact]
    public void Constructor_WithEmptyBuyerId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Trade(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Empty,
                EnergyType.Solar,
                100m,
                85m,
                Currency.EUR));
    }

    [Fact]
    public void Constructor_WhenSellerAndBuyerAreSame_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() =>
            new Trade(
                Guid.NewGuid(),
                userId,
                userId,
                EnergyType.Solar,
                100m,
                85m,
                Currency.EUR));
    }

    [Fact]
    public void Constructor_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Trade(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                0m,
                85m,
                Currency.EUR));
    }

    [Fact]
    public void Constructor_WithZeroPrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Trade(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                100m,
                0m,
                Currency.EUR));
    }
}