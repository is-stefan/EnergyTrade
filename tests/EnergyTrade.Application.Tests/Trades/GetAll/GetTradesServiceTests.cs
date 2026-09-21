using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Trades.GetAll;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Trades.GetAll;

public class GetTradesServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenTradesExist_ReturnsPagedTrades()
    {
        // Arrange
        var firstTrade = CreateTrade(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m);

        var secondTrade = CreateTrade(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Wind,
            200m);

        var repository = new FakeTradeRepository(
            new[] { firstTrade, secondTrade });

        var service = new GetTradesService(repository);

        // Act
        var result = await service.ExecuteAsync();

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);

        Assert.Equal(firstTrade.Id, result.Items[0].Id);
        Assert.Equal(secondTrade.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ReturnsRequestedPage()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();

        var trades = new List<Trade>
        {
            CreateTrade(sellerId, buyerId, EnergyType.Solar, 100m),
            CreateTrade(sellerId, buyerId, EnergyType.Solar, 200m),
            CreateTrade(sellerId, buyerId, EnergyType.Solar, 300m),
            CreateTrade(sellerId, buyerId, EnergyType.Solar, 400m),
            CreateTrade(sellerId, buyerId, EnergyType.Solar, 500m)
        };

        var repository = new FakeTradeRepository(trades);
        var service = new GetTradesService(repository);

        // Act
        var result = await service.ExecuteAsync(
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
    public async Task ExecuteAsync_WithSellerFilter_ReturnsOnlySellerTrades()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var otherSellerId = Guid.NewGuid();

        var trades = new List<Trade>
        {
            CreateTrade(sellerId, Guid.NewGuid(), EnergyType.Solar, 100m),
            CreateTrade(otherSellerId, Guid.NewGuid(), EnergyType.Wind, 200m)
        };

        var repository = new FakeTradeRepository(trades);
        var service = new GetTradesService(repository);

        // Act
        var result = await service.ExecuteAsync(
            sellerId: sellerId);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(sellerId, result.Items[0].SellerId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeTradeRepository(
            Array.Empty<Trade>());

        var service = new GetTradesService(repository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(page: 0));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeTradeRepository(
            Array.Empty<Trade>());

        var service = new GetTradesService(repository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(pageSize: 101));
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
            Guid? sellerId = null,
            Guid? buyerId = null,
            EnergyType? energyType = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _trades.AsEnumerable();

            if (sellerId.HasValue)
            {
                query = query.Where(
                    trade => trade.SellerId == sellerId.Value);
            }

            if (buyerId.HasValue)
            {
                query = query.Where(
                    trade => trade.BuyerId == buyerId.Value);
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
            Guid? sellerId = null,
            Guid? buyerId = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
        {
            var query = _trades.AsEnumerable();

            if (sellerId.HasValue)
            {
                query = query.Where(
                    trade => trade.SellerId == sellerId.Value);
            }

            if (buyerId.HasValue)
            {
                query = query.Where(
                    trade => trade.BuyerId == buyerId.Value);
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