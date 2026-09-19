using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.EnergyOffers.Create;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.Create;

public class CreateEnergyOfferServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesAndSavesEnergyOffer()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();
        var service = new CreateEnergyOfferService(repository);

        var sellerId = Guid.NewGuid();

        var deliveryStart = new DateTimeOffset(
            2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

        var deliveryEnd = new DateTimeOffset(
            2026, 10, 31, 0, 0, 0, TimeSpan.Zero);

        var request = new CreateEnergyOfferRequest(
            sellerId,
            EnergyType.Solar,
            100m,
            82.50m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.Equal(1, repository.AddCallCount);

        var savedOffer = Assert.IsType<EnergyOffer>(
            repository.AddedOffer);

        Assert.Equal(sellerId, savedOffer.SellerId);
        Assert.Equal(EnergyType.Solar, savedOffer.EnergyType);
        Assert.Equal(100m, savedOffer.QuantityMWh);
        Assert.Equal(82.50m, savedOffer.PricePerMWh);
        Assert.Equal(Currency.EUR, savedOffer.Currency);
        Assert.Equal(deliveryStart, savedOffer.DeliveryStart);
        Assert.Equal(deliveryEnd, savedOffer.DeliveryEnd);
        Assert.Equal(OfferStatus.Open, savedOffer.Status);

        Assert.Equal(savedOffer.Id, result.Id);
        Assert.Equal(savedOffer.Status, result.Status);
        Assert.Equal(savedOffer.CreatedAt, result.CreatedAt);
    }

    private sealed class FakeEnergyOfferRepository
        : IEnergyOfferRepository
    {
        public EnergyOffer? AddedOffer { get; private set; }

        public int AddCallCount { get; private set; }

        public Task AddAsync(
            EnergyOffer energyOffer,
            CancellationToken cancellationToken = default)
        {
            AddedOffer = energyOffer;
            AddCallCount++;

            return Task.CompletedTask;
        }

        public Task<EnergyOffer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EnergyOffer?>(null);
        }

        public Task<IReadOnlyList<EnergyOffer>> GetAllAsync(
        CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EnergyOffer>>(
                Array.Empty<EnergyOffer>());
        }

    }

    [Fact]
    public async Task ExecuteAsync_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();
        var service = new CreateEnergyOfferService(repository);

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateEnergyOfferRequest(
            Guid.NewGuid(),
            EnergyType.Wind,
            0m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(request));

        // Assert
        Assert.Equal("quantityMWh", exception.ParamName);

        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedOffer);
    }

    
}

