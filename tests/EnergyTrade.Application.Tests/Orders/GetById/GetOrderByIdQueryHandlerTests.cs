using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Orders.GetById;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Orders.GetById;

public class GetOrderByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderExistsAndBelongsToUser_ReturnsOrder()
    {
        // Arrange
        var order = CreateOrder();

        var repository = new FakeOrderRepository(order);

        var handler = new GetOrderByIdQueryHandler(repository);

        var query = new GetOrderByIdQuery(
            order.BuyerId,
            order.Id);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

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
    public async Task Handle_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeOrderRepository(null);

        var handler = new GetOrderByIdQueryHandler(repository);

        var query = new GetOrderByIdQuery(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenOrderBelongsToAnotherUser_ThrowsForbiddenException()
    {
        // Arrange
        var order = CreateOrder();

        var repository = new FakeOrderRepository(order);

        var handler = new GetOrderByIdQueryHandler(repository);

        var anotherUserId = Guid.NewGuid();

        var query = new GetOrderByIdQuery(
            anotherUserId,
            order.Id);

        // Act
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                query,
                CancellationToken.None));

        // Assert
        Assert.Equal(
            "The order does not belong to the authenticated user.",
            exception.Message);
    }

    private static Order CreateOrder()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new Order(
            Guid.NewGuid(),
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