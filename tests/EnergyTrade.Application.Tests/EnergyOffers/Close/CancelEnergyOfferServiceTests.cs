using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.EnergyOffers.Cancel;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.Cancel;

public class CancelEnergyOfferServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOfferExistsAndBelongsToUser_CancelsOfferAndSavesChanges()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var service = new CancelEnergyOfferService(repository);

        // Act
        var result = await service.ExecuteAsync(
            offer.SellerId,
            offer.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(OfferStatus.Cancelled, offer.Status);
        Assert.NotNull(offer.UpdatedAt);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(null);
        var service = new CancelEnergyOfferService(repository);

        // Act
        var result = await service.ExecuteAsync(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferBelongsToAnotherUser_ThrowsForbiddenException()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var service = new CancelEnergyOfferService(repository);

        var anotherUserId = Guid.NewGuid();

        // Act
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => service.ExecuteAsync(
                anotherUserId,
                offer.Id));

        // Assert
        Assert.Equal(
            "The energy offer does not belong to the authenticated user.",
            exception.Message);

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferIsAlreadyCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateValidOffer();
        offer.Cancel();

        var repository = new FakeEnergyOfferRepository(offer);
        var service = new CancelEnergyOfferService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(
                offer.SellerId,
                offer.Id));

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private static EnergyOffer CreateValidOffer()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
            Guid.NewGuid(),
            Guid.NewGuid(),
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
            Guid? sellerId = null,
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
            Guid? sellerId = null,
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