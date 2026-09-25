using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Orders.Create;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Orders.Create;

public class CreateOrderServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            request.BuyerId,
            "Test Portfolio",
            request.Currency);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var service = new CreateOrderService(
            repository,
            portfolioRepository);

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(OrderStatus.Open, result.Status);
        Assert.NotEqual(default, result.CreatedAt);

        Assert.NotNull(repository.AddedOrder);
        Assert.Equal(1, repository.AddCallCount);

        Assert.Equal(
            request.BuyerId,
            repository.AddedOrder.BuyerId);

        Assert.Equal(
            request.PortfolioId,
            repository.AddedOrder.PortfolioId);

        Assert.Equal(
            request.EnergyType,
            repository.AddedOrder.EnergyType);

        Assert.Equal(
            request.QuantityMWh,
            repository.AddedOrder.QuantityMWh);

        Assert.Equal(
            request.MaxPricePerMWh,
            repository.AddedOrder.MaxPricePerMWh);

        Assert.Equal(
            request.Currency,
            repository.AddedOrder.Currency);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeOrderRepository();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            0m,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            request.BuyerId,
            "Test Portfolio",
            request.Currency);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var service = new CreateOrderService(
            repository,
            portfolioRepository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidDeliveryRange_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeOrderRepository();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(10);
        var deliveryEnd = deliveryStart.AddDays(-1);

        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            request.BuyerId,
            "Test Portfolio",
            request.Currency);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var service = new CreateOrderService(
            repository,
            portfolioRepository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeOrderRepository();

        var portfolioRepository =
            new FakePortfolioRepository(null);

        var service = new CreateOrderService(
            repository,
            portfolioRepository);

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioIsClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeOrderRepository();

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var portfolio = new Portfolio(
            request.BuyerId,
            "Test Portfolio",
            request.Currency);

        portfolio.Close();

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var service = new CreateOrderService(
            repository,
            portfolioRepository);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, repository.AddCallCount);
    }

    private sealed class FakeOrderRepository
        : IOrderRepository
    {
        public Order? AddedOrder { get; private set; }

        public int AddCallCount { get; private set; }

        public Task AddAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            AddedOrder = order;
            AddCallCount++;

            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Order?>(null);
        }

        public Task<IReadOnlyList<Order>> GetAllAsync(
            Guid? buyerId = null,
            EnergyType? energyType = null,
            OrderStatus? status = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Order>>(
                Array.Empty<Order>());
        }

        public Task<int> CountAsync(
            Guid? buyerId = null,
            EnergyType? energyType = null,
            OrderStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
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