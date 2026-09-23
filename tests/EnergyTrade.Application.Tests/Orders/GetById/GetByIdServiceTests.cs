using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Orders.GetById;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Orders.GetById;

public class GetOrderByIdServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOrderExists_ReturnsOrder()
    {
        // Arrange
        var order = CreateOrder();

        var repository = new FakeOrderRepository(order);
        var service = new GetOrderByIdService(repository);

        // Act
        var result = await service.ExecuteAsync(order.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.BuyerId, result.BuyerId);
        Assert.Equal(order.EnergyType, result.EnergyType);
        Assert.Equal(order.QuantityMWh, result.QuantityMWh);
        Assert.Equal(order.MaxPricePerMWh, result.MaxPricePerMWh);
        Assert.Equal(order.Currency, result.Currency);
        Assert.Equal(order.DeliveryStart, result.DeliveryStart);
        Assert.Equal(order.DeliveryEnd, result.DeliveryEnd);
        Assert.Equal(order.Status, result.Status);
        Assert.Equal(order.CreatedAt, result.CreatedAt);
        Assert.Equal(order.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeOrderRepository(null);
        var service = new GetOrderByIdService(repository);

        // Act
        var result = await service.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    private static Order CreateOrder()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new Order(
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }

    private sealed class FakeOrderRepository
        : IOrderRepository
    {
        private readonly Order? _order;

        public FakeOrderRepository(Order? order)
        {
            _order = order;
        }

        public Task AddAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (_order?.Id == id)
            {
                return Task.FromResult<Order?>(_order);
            }

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