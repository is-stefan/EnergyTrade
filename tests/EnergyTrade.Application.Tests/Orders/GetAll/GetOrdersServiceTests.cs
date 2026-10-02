using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Orders.GetAll;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Orders.GetAll;

public class GetOrdersServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOrdersExist_ReturnsOnlyAuthenticatedUserOrders()
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

        var service = new GetOrdersService(repository);

        // Act
        var result = await service.ExecuteAsync(userId);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);

        Assert.Equal(firstOrder.Id, result.Items[0].Id);
        Assert.Equal(secondOrder.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ReturnsRequestedPage()
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
        var service = new GetOrdersService(repository);

        // Act
        var result = await service.ExecuteAsync(
            userId,
            page: 2,
            pageSize: 2);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);

        Assert.Equal(300m, result.Items[0].QuantityMWh);
        Assert.Equal(400m, result.Items[1].QuantityMWh);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOnlyAuthenticatedUserOrders()
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
        var service = new GetOrdersService(repository);

        // Act
        var result = await service.ExecuteAsync(userId);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(userId, result.Items[0].BuyerId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithEnergyTypeFilter_ReturnsOnlyMatchingOrders()
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
        var service = new GetOrdersService(repository);

        // Act
        var result = await service.ExecuteAsync(
            userId,
            energyType: EnergyType.Wind);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(EnergyType.Wind, result.Items[0].EnergyType);
        Assert.Equal(userId, result.Items[0].BuyerId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        var repository = new FakeOrderRepository(
            Array.Empty<Order>());

        var service = new GetOrdersService(repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(Guid.Empty));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeOrderRepository(
            Array.Empty<Order>());

        var service = new GetOrdersService(repository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                Guid.NewGuid(),
                page: 0));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeOrderRepository(
            Array.Empty<Order>());

        var service = new GetOrdersService(repository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                Guid.NewGuid(),
                pageSize: 101));
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