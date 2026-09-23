using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.EnergyOffers.Close;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.Close;

public class CloseEnergyOfferServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOfferExists_ClosesOfferAndSavesChanges()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var service = new CloseEnergyOfferService(repository);

        // Act
        var result = await service.ExecuteAsync(offer.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(OfferStatus.Closed, offer.Status);
        Assert.NotNull(offer.UpdatedAt);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(null);
        var service = new CloseEnergyOfferService(repository);

        // Act
        var result = await service.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferIsAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateValidOffer();
        offer.Close();

        var repository = new FakeEnergyOfferRepository(offer);
        var service = new CloseEnergyOfferService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(offer.Id));

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
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EnergyOffer>>(
                Array.Empty<EnergyOffer>());
        }

        public Task<int> CountAsync(
            OfferStatus? status = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }

        public Task<EnergyOffer?> FindMatchingAsync(
            Guid buyerId,
            EnergyType energyType,
            decimal quantityMWh,
            decimal maxPricePerMWh,
            Currency currency,
            DateTimeOffset deliveryStart,
            DateTimeOffset deliveryEnd,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EnergyOffer?>(null);
        }

    }
}