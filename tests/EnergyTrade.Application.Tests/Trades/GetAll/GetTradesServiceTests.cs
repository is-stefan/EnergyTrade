using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Trades.GetAll;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Trades.GetAll;

public class GetTradesServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenTradesExist_ReturnsOnlyAuthenticatedUserTrades()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var sellerTrade = CreateTrade(
            userId,
            otherUserId,
            EnergyType.Solar,
            100m);

        var buyerTrade = CreateTrade(
            otherUserId,
            userId,
            EnergyType.Wind,
            200m);

        var unrelatedTrade = CreateTrade(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            300m);

        var repository = new FakeTradeRepository(
            new[] { sellerTrade, buyerTrade, unrelatedTrade });

        var service = new GetTradesService(repository);

        // Act
        var result = await service.ExecuteAsync(userId);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);

        Assert.Contains(
            result.Items,
            x => x.Id == sellerTrade.Id);

        Assert.Contains(
            result.Items,
            x => x.Id == buyerTrade.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ReturnsRequestedPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var trades = new List<Trade>
        {
            CreateTrade(userId, otherUserId, EnergyType.Solar, 100m),
            CreateTrade(otherUserId, userId, EnergyType.Solar, 200m),
            CreateTrade(userId, otherUserId, EnergyType.Solar, 300m),
            CreateTrade(otherUserId, userId, EnergyType.Solar, 400m),
            CreateTrade(userId, otherUserId, EnergyType.Solar, 500m)
        };

        var repository = new FakeTradeRepository(trades);
        var service = new GetTradesService(repository);

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
    public async Task ExecuteAsync_WhenUserIsSeller_ReturnsTrade()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var trade = CreateTrade(
            userId,
            Guid.NewGuid(),
            EnergyType.Solar,
            100m);

        var repository = new FakeTradeRepository(
            new[] { trade });

        var service = new GetTradesService(repository);

        // Act
        var result = await service.ExecuteAsync(userId);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(userId, result.Items[0].SellerId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsBuyer_ReturnsTrade()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var trade = CreateTrade(
            Guid.NewGuid(),
            userId,
            EnergyType.Solar,
            100m);

        var repository = new FakeTradeRepository(
            new[] { trade });

        var service = new GetTradesService(repository);

        // Act
        var result = await service.ExecuteAsync(userId);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(userId, result.Items[0].BuyerId);
    }

    [Fact]
    public async Task ExecuteAsync_WithEnergyTypeFilter_ReturnsOnlyMatchingTrades()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var trades = new List<Trade>
        {
            CreateTrade(userId, otherUserId, EnergyType.Solar, 100m),
            CreateTrade(otherUserId, userId, EnergyType.Wind, 200m),
            CreateTrade(Guid.NewGuid(), Guid.NewGuid(), EnergyType.Wind, 300m)
        };

        var repository = new FakeTradeRepository(trades);
        var service = new GetTradesService(repository);

        // Act
        var result = await service.ExecuteAsync(
            userId,
            energyType: EnergyType.Wind);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(EnergyType.Wind, result.Items[0].EnergyType);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        var repository = new FakeTradeRepository(
            Array.Empty<Trade>());

        var service = new GetTradesService(repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(Guid.Empty));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeTradeRepository(
            Array.Empty<Trade>());

        var service = new GetTradesService(repository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                Guid.NewGuid(),
                page: 0));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeTradeRepository(
            Array.Empty<Trade>());

        var service = new GetTradesService(repository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                Guid.NewGuid(),
                pageSize: 101));
    }

    private static Trade CreateTrade(
        Guid sellerId,
        Guid buyerId,
        EnergyType energyType,
        decimal quantityMWh)
    {
        return new Trade(
            Guid.NewGuid(),
            sellerId,
            buyerId,
            energyType,
            quantityMWh,
            85m,
            Currency.EUR);
    }

    private sealed class FakeTradeRepository
        : ITradeRepository
    {
        private readonly IReadOnlyList<Trade> _trades;

        public FakeTradeRepository(
            IReadOnlyList<Trade> trades)
        {
            _trades = trades;
        }

        public Task AddAsync(
            Trade trade,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Trade?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var trade = _trades.FirstOrDefault(
                trade => trade.Id == id);

            return Task.FromResult(trade);
        }

        public Task<IReadOnlyList<Trade>> GetAllAsync(
            Guid? participantId = null,
            EnergyType? energyType = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _trades.AsEnumerable();

            if (participantId.HasValue)
            {
                query = query.Where(
                    trade =>
                        trade.SellerId == participantId.Value ||
                        trade.BuyerId == participantId.Value);
            }

            if (energyType.HasValue)
            {
                query = query.Where(
                    trade => trade.EnergyType == energyType.Value);
            }

            var result = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<IReadOnlyList<Trade>>(result);
        }

        public Task<int> CountAsync(
            Guid? participantId = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
        {
            var query = _trades.AsEnumerable();

            if (participantId.HasValue)
            {
                query = query.Where(
                    trade =>
                        trade.SellerId == participantId.Value ||
                        trade.BuyerId == participantId.Value);
            }

            if (energyType.HasValue)
            {
                query = query.Where(
                    trade => trade.EnergyType == energyType.Value);
            }

            return Task.FromResult(query.Count());
        }
    }
}