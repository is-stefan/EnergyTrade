using EnergyTrade.Application.Abstractions.Observability;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.EnergyOffers.Create;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.Create;

public class CreateEnergyOfferCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_CreatesAndSavesEnergyOffer()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();

        var sellerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var deliveryStart = new DateTimeOffset(
            2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

        var deliveryEnd = new DateTimeOffset(
            2026, 10, 31, 0, 0, 0, TimeSpan.Zero);

        var request = new CreateEnergyOfferRequest(
            portfolioId,
            EnergyType.Solar,
            100m,
            82.50m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            sellerId,
            "Test Portfolio",
            request.Currency);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var businessMetrics = new FakeBusinessMetrics();

        var handler = new CreateEnergyOfferCommandHandler(
            repository,
            portfolioRepository,
            businessMetrics);

        var command = CreateEnergyOfferCommand.FromRequest(
            sellerId,
            request);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

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

        Assert.Equal(1, businessMetrics.EnergyOffersCreated);
    }

    [Fact]
    public async Task Handle_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();

        var sellerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateEnergyOfferRequest(
            portfolioId,
            EnergyType.Wind,
            0m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            sellerId,
            "Test Portfolio",
            request.Currency);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var businessMetrics = new FakeBusinessMetrics();

        var handler = new CreateEnergyOfferCommandHandler(
            repository,
            portfolioRepository,
            businessMetrics);

        var command = CreateEnergyOfferCommand.FromRequest(
            sellerId,
            request);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => handler.Handle(
                    command,
                    CancellationToken.None));

        // Assert
        Assert.Equal("quantityMWh", exception.ParamName);

        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedOffer);
        Assert.Equal(0, businessMetrics.EnergyOffersCreated);
    }

    [Fact]
    public async Task Handle_WhenPortfolioDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();

        var sellerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var portfolioRepository =
            new FakePortfolioRepository(null);

        var businessMetrics = new FakeBusinessMetrics();

        var handler = new CreateEnergyOfferCommandHandler(
            repository,
            portfolioRepository,
            businessMetrics);

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateEnergyOfferRequest(
            portfolioId,
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var command = CreateEnergyOfferCommand.FromRequest(
            sellerId,
            request);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedOffer);
        Assert.Equal(0, businessMetrics.EnergyOffersCreated);
    }

    [Fact]
    public async Task Handle_WhenPortfolioIsClosed_ThrowsConflictException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();

        var sellerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateEnergyOfferRequest(
            portfolioId,
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            sellerId,
            "Test Portfolio",
            request.Currency);

        portfolio.Close();

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var businessMetrics = new FakeBusinessMetrics();

        var handler = new CreateEnergyOfferCommandHandler(
            repository,
            portfolioRepository,
            businessMetrics);

        var command = CreateEnergyOfferCommand.FromRequest(
            sellerId,
            request);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedOffer);
        Assert.Equal(0, businessMetrics.EnergyOffersCreated);
    }

    [Fact]
    public async Task Handle_WhenPortfolioBelongsToAnotherUser_ThrowsForbiddenException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository();

        var authenticatedUserId = Guid.NewGuid();
        var portfolioOwnerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateEnergyOfferRequest(
            portfolioId,
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            portfolioOwnerId,
            "Test Portfolio",
            request.Currency);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var businessMetrics = new FakeBusinessMetrics();

        var handler = new CreateEnergyOfferCommandHandler(
            repository,
            portfolioRepository,
            businessMetrics);

        var command = CreateEnergyOfferCommand.FromRequest(
            authenticatedUserId,
            request);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedOffer);
        Assert.Equal(0, businessMetrics.EnergyOffersCreated);
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

    private sealed class FakeBusinessMetrics : IBusinessMetrics
    {
        public int OrdersCreated { get; private set; }
        public int EnergyOffersCreated { get; private set; }
        public int TradesCompleted { get; private set; }

        public void OrderCreated()
        {
            OrdersCreated++;
        }

        public void EnergyOfferCreated()
        {
            EnergyOffersCreated++;
        }

        public void TradeCompleted()
        {
            TradesCompleted++;
        }

        public void RecordOrderMatchingDuration(double milliseconds)
        {
        }
    }
}