using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Trades.Create;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Trades.Create;

public class CreateTradeServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOfferExists_CreatesTradeAndClosesOffer()
    {
        // Arrange
        var offer = CreateValidOffer();
        var buyerId = Guid.NewGuid();

        var energyOfferRepository =
            new FakeEnergyOfferRepository(offer);

        var tradeRepository =
            new FakeTradeRepository();

        var service = new CreateTradeService(
            energyOfferRepository,
            tradeRepository);

        var request = new CreateTradeRequest(
            offer.Id,
            buyerId);

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(offer.Id, result.EnergyOfferId);
        Assert.Equal(offer.SellerId, result.SellerId);
        Assert.Equal(buyerId, result.BuyerId);
        Assert.Equal(offer.EnergyType, result.EnergyType);
        Assert.Equal(offer.QuantityMWh, result.QuantityMWh);
        Assert.Equal(offer.PricePerMWh, result.PricePerMWh);
        Assert.Equal(offer.Currency, result.Currency);

        Assert.Equal(OfferStatus.Closed, offer.Status);

        Assert.NotNull(tradeRepository.AddedTrade);
        Assert.Equal(1, tradeRepository.AddCallCount);
        Assert.Equal(1, energyOfferRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferDoesNotExist_ReturnsNull()
    {
        // Arrange
        var energyOfferRepository =
            new FakeEnergyOfferRepository(null);

        var tradeRepository =
            new FakeTradeRepository();

        var service = new CreateTradeService(
            energyOfferRepository,
            tradeRepository);

        var request = new CreateTradeRequest(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.Null(result);
        Assert.Equal(0, tradeRepository.AddCallCount);
        Assert.Equal(0, energyOfferRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferIsNotOpen_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateValidOffer();
        offer.Close();

        var energyOfferRepository =
            new FakeEnergyOfferRepository(offer);

        var tradeRepository =
            new FakeTradeRepository();

        var service = new CreateTradeService(
            energyOfferRepository,
            tradeRepository);

        var request = new CreateTradeRequest(
            offer.Id,
            Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, tradeRepository.AddCallCount);
        Assert.Equal(0, energyOfferRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBuyerIsSeller_ThrowsInvalidOperationException()
    {
        // Arrange
        var offer = CreateValidOffer();

        var energyOfferRepository =
            new FakeEnergyOfferRepository(offer);

        var tradeRepository =
            new FakeTradeRepository();

        var service = new CreateTradeService(
            energyOfferRepository,
            tradeRepository);

        var request = new CreateTradeRequest(
            offer.Id,
            offer.SellerId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, tradeRepository.AddCallCount);
        Assert.Equal(0, energyOfferRepository.SaveChangesCallCount);
    }

    private static EnergyOffer CreateValidOffer()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            85m,
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
    }

    private sealed class FakeTradeRepository
        : ITradeRepository
    {
        public Trade? AddedTrade { get; private set; }

        public int AddCallCount { get; private set; }

        public Task AddAsync(
            Trade trade,
            CancellationToken cancellationToken = default)
        {
            AddedTrade = trade;
            AddCallCount++;

            return Task.CompletedTask;
        }
    }
}