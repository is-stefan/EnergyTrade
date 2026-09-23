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
        var service = new CreateOrderService(repository);

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(OrderStatus.Open, result.Status);
        Assert.NotEqual(default, result.CreatedAt);

        Assert.NotNull(repository.AddedOrder);
        Assert.Equal(1, repository.AddCallCount);

        Assert.Equal(request.BuyerId, repository.AddedOrder.BuyerId);
        Assert.Equal(request.EnergyType, repository.AddedOrder.EnergyType);
        Assert.Equal(request.QuantityMWh, repository.AddedOrder.QuantityMWh);
        Assert.Equal(request.MaxPricePerMWh, repository.AddedOrder.MaxPricePerMWh);
        Assert.Equal(request.Currency, repository.AddedOrder.Currency);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new CreateOrderService(repository);

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            EnergyType.Solar,
            0m,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

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
        var service = new CreateOrderService(repository);

        var deliveryStart = DateTimeOffset.UtcNow.AddDays(10);
        var deliveryEnd = deliveryStart.AddDays(-1);

        var request = new CreateOrderRequest(
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
}