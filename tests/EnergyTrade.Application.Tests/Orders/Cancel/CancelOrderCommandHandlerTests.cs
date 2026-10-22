using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Orders.Cancel;
using EnergyTrade.Domain.Common.Exceptions;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Orders.Cancel;

public class CancelOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderExistsAndBelongsToUser_CancelsOrderAndSavesChanges()
    {
        // Arrange
        var order = CreateValidOrder();

        var repository = new FakeOrderRepository(order);
        var handler = new CancelOrderCommandHandler(repository);

        var command = new CancelOrderCommand(
            order.BuyerId,
            order.Id);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.UpdatedAt);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new FakeOrderRepository(null);
        var handler = new CancelOrderCommandHandler(repository);

        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderBelongsToAnotherUser_ThrowsForbiddenException()
    {
        // Arrange
        var order = CreateValidOrder();

        var repository = new FakeOrderRepository(order);
        var handler = new CancelOrderCommandHandler(repository);

        var anotherUserId = Guid.NewGuid();

        var command = new CancelOrderCommand(
            anotherUserId,
            order.Id);

        // Act
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        // Assert
        Assert.Equal(
            "The order does not belong to the authenticated user.",
            exception.Message);

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderIsAlreadyCancelled_ThrowsBusinessRuleException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Cancel();

        var repository = new FakeOrderRepository(order);
        var handler = new CancelOrderCommandHandler(repository);

        var command = new CancelOrderCommand(
            order.BuyerId,
            order.Id);

        // Act + Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderIsFilled_ThrowsBusinessRuleException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Fill();

        var repository = new FakeOrderRepository(order);
        var handler = new CancelOrderCommandHandler(repository);

        var command = new CancelOrderCommand(
            order.BuyerId,
            order.Id);

        // Act + Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private static Order CreateValidOrder()
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

        public int SaveChangesCallCount { get; private set; }

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
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}