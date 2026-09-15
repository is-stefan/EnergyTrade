using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Tests.Entities;

public class EnergyOfferTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesOpenOffer()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var deliveryStart = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

        var deliveryEnd = new DateTimeOffset(2026, 10, 31, 0, 0, 0, TimeSpan.Zero);

        // Act
        var offer = new EnergyOffer(
            sellerId,
            EnergyType.Solar,
            100m,
            82.5m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
        
        // Assert
        Assert.NotEqual(Guid.Empty, offer.Id);
        Assert.Equal(sellerId, offer.SellerId);
        Assert.Equal(EnergyType.Solar, offer.EnergyType);
        Assert.Equal(100m, offer.QuantityMWh);
        Assert.Equal(82.50m, offer.PricePerMWh);
        Assert.Equal(Currency.EUR, offer.Currency);
        Assert.Equal(deliveryStart, offer.DeliveryStart);
        Assert.Equal(deliveryEnd, offer.DeliveryEnd);
        Assert.Equal(OfferStatus.Open, offer.Status);

        Assert.NotEqual(default, offer.CreatedAt);
        Assert.Null(offer.UpdatedAt);

    }

    [Fact]
    public void Constructor_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EnergyOffer(
                sellerId,
                EnergyType.Wind,
                0m,
                80m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));

        // Assert
        Assert.Equal("quantityMWh", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithZeroPrice_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EnergyOffer(
                sellerId,
                EnergyType.Hydro,
                100m,
                0m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));

        // Assert
        Assert.Equal("pricePerMWh", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDeliveryEndBeforeStart_ThrowsArgumentException()
    {
        // Arrange
        var sellerId = Guid.NewGuid();

        var deliveryStart = new DateTimeOffset(
            2026, 10, 31, 0, 0, 0, TimeSpan.Zero);

        var deliveryEnd = new DateTimeOffset(
            2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new EnergyOffer(
                sellerId,
                EnergyType.Solar,
                100m,
                80m,
                Currency.EUR,
                deliveryStart,
                deliveryEnd));
    }

    [Fact]
    public void Update_WhenOfferIsOpen_UpdatesOfferData()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var offer = new EnergyOffer(
            sellerId,
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var newDeliveryStart = deliveryStart.AddDays(5);
        var newDeliveryEnd = deliveryEnd.AddDays(5);

        // Act
        offer.Update(
            EnergyType.Wind,
            150m,
            90m,
            Currency.USD,
            newDeliveryStart,
            newDeliveryEnd);

        // Assert
        Assert.Equal(EnergyType.Wind, offer.EnergyType);
        Assert.Equal(150m, offer.QuantityMWh);
        Assert.Equal(90m, offer.PricePerMWh);
        Assert.Equal(Currency.USD, offer.Currency);
        Assert.Equal(newDeliveryStart, offer.DeliveryStart);
        Assert.Equal(newDeliveryEnd, offer.DeliveryEnd);
        Assert.NotNull(offer.UpdatedAt);
    }

    [Fact]
    public void Close_WhenOfferIsOpen_ChangesStatusToClosed()
    {
        // Arrange
        var offer = CreateValidOffer();

        // Act
        offer.Close();

        // Assert
        Assert.Equal(OfferStatus.Closed, offer.Status);
        Assert.NotNull(offer.UpdatedAt);
    }

    [Fact]
    public void Cancel_WhenOfferIsOpen_ChangesStatusToCancelled()
    {
        // Arrange
        var offer = CreateValidOffer();

        // Act
        offer.Cancel();

        // Assert
        Assert.Equal(OfferStatus.Cancelled, offer.Status);
        Assert.NotNull(offer.UpdatedAt);
    }

    [Fact]
    public void Update_WhenOfferIsClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateValidOffer();
        offer.Close();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            offer.Update(
                EnergyType.Wind,
                150m,
                90m,
                Currency.USD,
                DateTimeOffset.UtcNow.AddDays(5),
                DateTimeOffset.UtcNow.AddDays(30)));
    }

    [Fact]
    public void Cancel_WhenOfferIsClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateValidOffer();
        offer.Close();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            offer.Cancel());
    }

        private static EnergyOffer CreateValidOffer()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }
}