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

        var sellerId = Guid.NewGuid();

        var deliveryStart = new DateTimeOffset(
            2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

        var deliveryEnd = new DateTimeOffset(
            2026, 10, 31, 0, 0, 0, TimeSpan.Zero);

        var request = new CreateEnergyOfferRequest(
            sellerId,
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            82.50m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            request.SellerId,
            "Test Portfolio",
            request.Currency);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var service = new CreateEnergyOfferService(
            repository,
            portfolioRepository);

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.Equal(1, repository.AddCallCount);

        var savedOffer = Assert.IsType<EnergyOffer>(
            repository.AddedOffer);

        Assert.Equal(sellerId, savedOffer.SellerId);
        Assert.Equal(request.PortfolioId, savedOffer.PortfolioId);
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

    [Fact]
    public async Task ExecuteAsync_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateEnergyOfferRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Wind,
            0m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            request.SellerId,
            "Test Portfolio",
            request.Currency);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var service = new CreateEnergyOfferService(
            repository,
            portfolioRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.ExecuteAsync(request));

        // Assert
        Assert.Equal("quantityMWh", exception.ParamName);

        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedOffer);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();

        var portfolioRepository =
            new FakePortfolioRepository(null);

        var service = new CreateEnergyOfferService(
            repository,
            portfolioRepository);

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateEnergyOfferRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedOffer);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioIsClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateEnergyOfferRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            request.SellerId,
            "Test Portfolio",
            request.Currency);

        portfolio.Close();

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var service = new CreateEnergyOfferService(
            repository,
            portfolioRepository);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedOffer);
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

    private sealed class FakePortfolioRepository
        : IPortfolioRepository
    {
        private readonly Portfolio? _portfolio;

        public FakePortfolioRepository(
            Portfolio? portfolio)
        {
            _portfolio = portfolio;
        }

        public Task AddAsync(
            Portfolio portfolio,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Portfolio?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_portfolio);
        }

        public Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Portfolio>>(
                Array.Empty<Portfolio>());
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}