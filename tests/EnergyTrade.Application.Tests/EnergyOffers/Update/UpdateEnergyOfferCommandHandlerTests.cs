using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.EnergyOffers.Update;
using EnergyTrade.Domain.Common.Exceptions;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.Update;

public class UpdateEnergyOfferCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOfferExistsAndBelongsToUser_UpdatesOfferAndSavesChanges()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var handler = new UpdateEnergyOfferCommandHandler(repository);

        var newDeliveryStart = DateTimeOffset.UtcNow.AddDays(10);
        var newDeliveryEnd = newDeliveryStart.AddDays(20);

        var request = new UpdateEnergyOfferRequest(
            EnergyType.Wind,
            300m,
            95m,
            Currency.USD,
            newDeliveryStart,
            newDeliveryEnd);

        var command = UpdateEnergyOfferCommand.FromRequest(
            offer.SellerId,
            offer.Id,
            request);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

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
    public async Task Handle_WhenOfferDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(null);
        var handler = new UpdateEnergyOfferCommandHandler(repository);

        var request = CreateValidRequest();

        var command = UpdateEnergyOfferCommand.FromRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            request);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOfferBelongsToAnotherUser_ThrowsForbiddenException()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var handler = new UpdateEnergyOfferCommandHandler(repository);

        var request = CreateValidRequest();
        var anotherUserId = Guid.NewGuid();

        var command = UpdateEnergyOfferCommand.FromRequest(
            anotherUserId,
            offer.Id,
            request);

        // Act
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        // Assert
        Assert.Equal(
            "The energy offer does not belong to the authenticated user.",
            exception.Message);

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var handler = new UpdateEnergyOfferCommandHandler(repository);

        var request = new UpdateEnergyOfferRequest(
            EnergyType.Solar,
            0m,
            80m,
            Currency.EUR,
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(30));

        var command = UpdateEnergyOfferCommand.FromRequest(
            offer.SellerId,
            offer.Id,
            request);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOfferIsClosed_ThrowsBusinessRuleException()
    {
        // Arrange
        var offer = CreateValidOffer();
        offer.Close();

        var repository = new FakeEnergyOfferRepository(offer);
        var handler = new UpdateEnergyOfferCommandHandler(repository);

        var request = CreateValidRequest();

        var command = UpdateEnergyOfferCommand.FromRequest(
            offer.SellerId,
            offer.Id,
            request);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private static EnergyOffer CreateValidOffer()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
            Guid.NewGuid(),
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