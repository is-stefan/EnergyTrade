using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.EnergyOffers.Update;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.Update;

public class UpdateEnergyOfferServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOfferExists_UpdatesOfferAndSavesChanges()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var service = new UpdateEnergyOfferService(repository);

        var newDeliveryStart = DateTimeOffset.UtcNow.AddDays(10);
        var newDeliveryEnd = newDeliveryStart.AddDays(20);

        var request = new UpdateEnergyOfferRequest(
            EnergyType.Wind,
            300m,
            95m,
            Currency.USD,
            newDeliveryStart,
            newDeliveryEnd);

        // Act
        var result = await service.ExecuteAsync(
            offer.Id,
            request);

        // Assert
        Assert.True(result);

        Assert.Equal(EnergyType.Wind, offer.EnergyType);
        Assert.Equal(300m, offer.QuantityMWh);
        Assert.Equal(95m, offer.PricePerMWh);
        Assert.Equal(Currency.USD, offer.Currency);
        Assert.Equal(newDeliveryStart, offer.DeliveryStart);
        Assert.Equal(newDeliveryEnd, offer.DeliveryEnd);
        Assert.NotNull(offer.UpdatedAt);

        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(null);
        var service = new UpdateEnergyOfferService(repository);

        var request = CreateValidRequest();

        // Act
        var result = await service.ExecuteAsync(
            Guid.NewGuid(),
            request);

        // Assert
        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var service = new UpdateEnergyOfferService(repository);

        var request = new UpdateEnergyOfferRequest(
            EnergyType.Solar,
            0m,
            80m,
            Currency.EUR,
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(30));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                offer.Id,
                request));

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferIsClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateValidOffer();
        offer.Close();

        var repository = new FakeEnergyOfferRepository(offer);
        var service = new UpdateEnergyOfferService(repository);

        var request = CreateValidRequest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(
                offer.Id,
                request));

        Assert.Equal(0, repository.SaveChangesCallCount);
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

    private static UpdateEnergyOfferRequest CreateValidRequest()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(5);
        var deliveryEnd = deliveryStart.AddDays(20);

        return new UpdateEnergyOfferRequest(
            EnergyType.Wind,
            200m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }

    private sealed class FakeEnergyOfferRepository
        : IEnergyOfferRepository
    {
        private readonly EnergyOffer? _offer;

        public int SaveChangesCallCount { get; private set; }

        public FakeEnergyOfferRepository(EnergyOffer? offer)
        {
            _offer = offer;
        }

        public Task AddAsync(
            EnergyOffer energyOffer,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<EnergyOffer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (_offer?.Id == id)
            {
                return Task.FromResult<EnergyOffer?>(_offer);
            }

            return Task.FromResult<EnergyOffer?>(null);
        }

        public Task<IReadOnlyList<EnergyOffer>> GetAllAsync(
            OfferStatus? status = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EnergyOffer>>(
                Array.Empty<EnergyOffer>());
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}