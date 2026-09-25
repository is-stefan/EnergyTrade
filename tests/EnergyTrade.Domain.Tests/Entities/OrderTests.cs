using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesOpenOrder()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        // Act
        var order = new Order(
            buyerId,
            portfolioId,
            EnergyType.Solar,
            100m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            order.Id);

        Assert.Equal(
            buyerId,
            order.BuyerId);

        Assert.Equal(
            portfolioId,
            order.PortfolioId);

        Assert.Equal(
            EnergyType.Solar,
            order.EnergyType);

        Assert.Equal(
            100m,
            order.QuantityMWh);

        Assert.Equal(
            90m,
            order.MaxPricePerMWh);

        Assert.Equal(
            Currency.EUR,
            order.Currency);

        Assert.Equal(
            deliveryStart,
            order.DeliveryStart);

        Assert.Equal(
            deliveryEnd,
            order.DeliveryEnd);

        Assert.Equal(
            OrderStatus.Open,
            order.Status);

        Assert.NotEqual(
            default,
            order.CreatedAt);

        Assert.Null(order.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyBuyerId_ThrowsArgumentException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        Assert.Throws<ArgumentException>(() =>
            new Order(
                Guid.Empty,
                Guid.NewGuid(),
                EnergyType.Solar,
                100m,
                90m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Constructor_WithEmptyPortfolioId_ThrowsArgumentException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        Assert.Throws<ArgumentException>(() =>
            new Order(
                Guid.NewGuid(),
                Guid.Empty,
                EnergyType.Solar,
                100m,
                90m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Constructor_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Order(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                0m,
                90m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Constructor_WithNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Order(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                -10m,
                90m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Constructor_WithZeroMaxPrice_ThrowsArgumentOutOfRangeException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Order(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                100m,
                0m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Constructor_WithInvalidDeliveryRange_ThrowsArgumentException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(10);

        var deliveryEnd =
            deliveryStart.AddDays(-1);

        Assert.Throws<ArgumentException>(() =>
            new Order(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                100m,
                90m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Fill_WhenOrderIsOpen_SetsStatusToFilled()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        order.Fill();

        // Assert
        Assert.Equal(
            OrderStatus.Filled,
            order.Status);

        Assert.NotNull(order.UpdatedAt);
    }

    [Fact]
    public void Cancel_WhenOrderIsOpen_SetsStatusToCancelled()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(
            OrderStatus.Cancelled,
            order.Status);

        Assert.NotNull(order.UpdatedAt);
    }

    [Fact]
    public void Fill_WhenOrderIsAlreadyFilled_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateOrder();

        order.Fill();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => order.Fill());
    }

    [Fact]
    public void Cancel_WhenOrderIsAlreadyCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateOrder();

        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => order.Cancel());
    }

    [Fact]
    public void Cancel_WhenOrderIsFilled_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateOrder();

        order.Fill();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => order.Cancel());
    }

    [Fact]
    public void Fill_WhenOrderIsCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateOrder();

        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => order.Fill());
    }

    private static Order CreateOrder()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        return new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }
}