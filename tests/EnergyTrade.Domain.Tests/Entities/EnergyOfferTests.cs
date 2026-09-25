using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Tests.Entities;

public class EnergyOfferTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesOpenEnergyOffer()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        // Act
        var offer = new EnergyOffer(
            sellerId,
            portfolioId,
            EnergyType.Solar,
            150m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            offer.Id);

        Assert.Equal(
            sellerId,
            offer.SellerId);

        Assert.Equal(
            portfolioId,
            offer.PortfolioId);

        Assert.Equal(
            EnergyType.Solar,
            offer.EnergyType);

        Assert.Equal(
            150m,
            offer.QuantityMWh);

        Assert.Equal(
            80m,
            offer.PricePerMWh);

        Assert.Equal(
            Currency.EUR,
            offer.Currency);

        Assert.Equal(
            deliveryStart,
            offer.DeliveryStart);

        Assert.Equal(
            deliveryEnd,
            offer.DeliveryEnd);

        Assert.Equal(
            OfferStatus.Open,
            offer.Status);

        Assert.NotEqual(
            default,
            offer.CreatedAt);

        Assert.Null(offer.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithEmptySellerId_ThrowsArgumentException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        Assert.Throws<ArgumentException>(() =>
            new EnergyOffer(
                Guid.Empty,
                Guid.NewGuid(),
                EnergyType.Solar,
                150m,
                80m,
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
            new EnergyOffer(
                Guid.NewGuid(),
                Guid.Empty,
                EnergyType.Solar,
                150m,
                80m,
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
            new EnergyOffer(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                0m,
                80m,
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
            new EnergyOffer(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                -10m,
                80m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Constructor_WithZeroPrice_ThrowsArgumentOutOfRangeException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EnergyOffer(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                150m,
                0m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EnergyOffer(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                150m,
                -10m,
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
            new EnergyOffer(
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnergyType.Solar,
                150m,
                80m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Close_WhenOfferIsOpen_SetsStatusToClosed()
    {
        // Arrange
        var offer = CreateOffer();

        // Act
        offer.Close();

        // Assert
        Assert.Equal(
            OfferStatus.Closed,
            offer.Status);

        Assert.NotNull(offer.UpdatedAt);
    }

    [Fact]
    public void Cancel_WhenOfferIsOpen_SetsStatusToCancelled()
    {
        // Arrange
        var offer = CreateOffer();

        // Act
        offer.Cancel();

        // Assert
        Assert.Equal(
            OfferStatus.Cancelled,
            offer.Status);

        Assert.NotNull(offer.UpdatedAt);
    }

    [Fact]
    public void Close_WhenOfferIsAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateOffer();

        offer.Close();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => offer.Close());
    }

    [Fact]
    public void Cancel_WhenOfferIsAlreadyCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateOffer();

        offer.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => offer.Cancel());
    }

    [Fact]
    public void Cancel_WhenOfferIsClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateOffer();

        offer.Close();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => offer.Cancel());
    }

    [Fact]
    public void Close_WhenOfferIsCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateOffer();

        offer.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => offer.Close());
    }

    private static EnergyOffer CreateOffer()
    {
        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(10);

        return new EnergyOffer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            150m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }
}