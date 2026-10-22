using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Orders.GetAll;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Orders.GetAll;

public class GetOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrdersExist_ReturnsOnlyAuthenticatedUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var firstOrder = CreateOrder(
            userId,
            EnergyType.Solar,
            100m);

        var secondOrder = CreateOrder(
            userId,
            EnergyType.Wind,
            200m);

        var otherUserOrder = CreateOrder(
            otherUserId,
            EnergyType.Solar,
            300m);

        var repository = new FakeOrderRepository(
            new[] { firstOrder, secondOrder, otherUserOrder });

        var handler = new GetOrdersQueryHandler(repository);

        var query = new GetOrdersQuery(userId);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);

        Assert.Equal(firstOrder.Id, result.Items[0].Id);
        Assert.Equal(secondOrder.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsRequestedPage()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var orders = new List<Order>
        {
            CreateOrder(userId, EnergyType.Solar, 100m),
            CreateOrder(userId, EnergyType.Solar, 200m),
            CreateOrder(userId, EnergyType.Solar, 300m),
            CreateOrder(userId, EnergyType.Solar, 400m),
            CreateOrder(userId, EnergyType.Solar, 500m)
        };

        var repository = new FakeOrderRepository(orders);
        var handler = new GetOrdersQueryHandler(repository);

        var query = new GetOrdersQuery(
            userId,
            Page: 2,
            PageSize: 2);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);

        Assert.Equal(300m, result.Items[0].QuantityMWh);
        Assert.Equal(400m, result.Items[1].QuantityMWh);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyAuthenticatedUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var orders = new List<Order>
        {
            CreateOrder(userId, EnergyType.Solar, 100m),
            CreateOrder(otherUserId, EnergyType.Wind, 200m)
        };

        var repository = new FakeOrderRepository(orders);
        var handler = new GetOrdersQueryHandler(repository);

        var query = new GetOrdersQuery(userId);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(userId, result.Items[0].BuyerId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithEnergyTypeFilter_ReturnsOnlyMatchingOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var orders = new List<Order>
        {
            CreateOrder(userId, EnergyType.Solar, 100m),
            CreateOrder(userId, EnergyType.Wind, 200m),
            CreateOrder(Guid.NewGuid(), EnergyType.Wind, 300m)
        };

        var repository = new FakeOrderRepository(orders);
        var handler = new GetOrdersQueryHandler(repository);

        var query = new GetOrdersQuery(
            userId,
            EnergyType: EnergyType.Wind);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(EnergyType.Wind, result.Items[0].EnergyType);
        Assert.Equal(userId, result.Items[0].BuyerId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeOrderRepository(
            Array.Empty<Order>());

        var handler = new GetOrdersQueryHandler(repository);

        var query = new GetOrdersQuery(Guid.Empty);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(
                query,
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeOrderRepository(
            Array.Empty<Order>());

        var handler = new GetOrdersQueryHandler(repository);

        var query = new GetOrdersQuery(
            Guid.NewGuid(),
            Page: 0);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.Handle(
                query,
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeOrderRepository(
            Array.Empty<Order>());

        var handler = new GetOrdersQueryHandler(repository);

        var query = new GetOrdersQuery(
            Guid.NewGuid(),
            PageSize: 101);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.Handle(
                query,
                CancellationToken.None));
    }

    private static Order CreateOrder(
        Guid buyerId,
        EnergyType energyType,
        decimal quantityMWh)
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new Order(
            buyerId,
            Guid.NewGuid(),
            energyType,
            quantityMWh,
            85m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }

    private sealed class FakeOrderRepository
        : IOrderRepository
    {
        private readonly IReadOnlyList<Order> _orders;

        public FakeOrderRepository(
            IReadOnlyList<Order> orders)
        {
            _orders = orders;
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
            var order = _orders.FirstOrDefault(
                order => order.Id == id);

            return Task.FromResult(order);
        }

        public Task<IReadOnlyList<Order>> GetAllAsync(
            Guid? buyerId = null,
            EnergyType? energyType = null,
            OrderStatus? status = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _orders.AsEnumerable();

            if (buyerId.HasValue)
            {
                query = query.Where(
                    order => order.BuyerId == buyerId.Value);
            }

            if (energyType.HasValue)
            {
                query = query.Where(
                    order => order.EnergyType == energyType.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(
                    order => order.Status == status.Value);
            }

            var result = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<IReadOnlyList<Order>>(result);
        }

        public Task<int> CountAsync(
            Guid? buyerId = null,
            EnergyType? energyType = null,
            OrderStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _orders.AsEnumerable();

            if (buyerId.HasValue)
            {
                query = query.Where(
                    order => order.BuyerId == buyerId.Value);
            }

            if (energyType.HasValue)
            {
                query = query.Where(
                    order => order.EnergyType == energyType.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(
                    order => order.Status == status.Value);
            }

            return Task.FromResult(query.Count());
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}