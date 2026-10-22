using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Trades.GetAll;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Trades.GetAll;

public class GetTradesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenTradesExist_ReturnsOnlyAuthenticatedUserTrades()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var sellerTrade = CreateTrade(userId, otherUserId, EnergyType.Solar, 100m);
        var buyerTrade = CreateTrade(otherUserId, userId, EnergyType.Wind, 200m);
        var unrelatedTrade = CreateTrade(Guid.NewGuid(), Guid.NewGuid(), EnergyType.Solar, 300m);

        var handler = new GetTradesQueryHandler(
            new FakeTradeRepository(new[] { sellerTrade, buyerTrade, unrelatedTrade }));

        var result = await handler.Handle(
            new GetTradesQuery(userId),
            CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, x => x.Id == sellerTrade.Id);
        Assert.Contains(result.Items, x => x.Id == buyerTrade.Id);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsRequestedPage()
    {
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

        var handler = new GetTradesQueryHandler(new FakeTradeRepository(trades));

        var result = await handler.Handle(
            new GetTradesQuery(userId, Page: 2, PageSize: 2),
            CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(300m, result.Items[0].QuantityMWh);
        Assert.Equal(400m, result.Items[1].QuantityMWh);
    }

    [Fact]
    public async Task Handle_WhenUserIsSeller_ReturnsTrade()
    {
        var userId = Guid.NewGuid();
        var trade = CreateTrade(userId, Guid.NewGuid(), EnergyType.Solar, 100m);
        var handler = new GetTradesQueryHandler(new FakeTradeRepository(new[] { trade }));

        var result = await handler.Handle(new GetTradesQuery(userId), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(userId, result.Items[0].SellerId);
    }

    [Fact]
    public async Task Handle_WhenUserIsBuyer_ReturnsTrade()
    {
        var userId = Guid.NewGuid();
        var trade = CreateTrade(Guid.NewGuid(), userId, EnergyType.Solar, 100m);
        var handler = new GetTradesQueryHandler(new FakeTradeRepository(new[] { trade }));

        var result = await handler.Handle(new GetTradesQuery(userId), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(userId, result.Items[0].BuyerId);
    }

    [Fact]
    public async Task Handle_WithEnergyTypeFilter_ReturnsOnlyMatchingTrades()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var trades = new List<Trade>
        {
            CreateTrade(userId, otherUserId, EnergyType.Solar, 100m),
            CreateTrade(otherUserId, userId, EnergyType.Wind, 200m),
            CreateTrade(Guid.NewGuid(), Guid.NewGuid(), EnergyType.Wind, 300m)
        };

        var handler = new GetTradesQueryHandler(new FakeTradeRepository(trades));

        var result = await handler.Handle(
            new GetTradesQuery(userId, EnergyType.Wind),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(EnergyType.Wind, result.Items[0].EnergyType);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ThrowsArgumentException()
    {
        var handler = new GetTradesQueryHandler(new FakeTradeRepository(Array.Empty<Trade>()));
        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new GetTradesQuery(Guid.Empty), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        var handler = new GetTradesQueryHandler(new FakeTradeRepository(Array.Empty<Trade>()));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.Handle(new GetTradesQuery(Guid.NewGuid(), Page: 0), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        var handler = new GetTradesQueryHandler(new FakeTradeRepository(Array.Empty<Trade>()));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.Handle(new GetTradesQuery(Guid.NewGuid(), PageSize: 101), CancellationToken.None));
    }

    private static Trade CreateTrade(Guid sellerId, Guid buyerId, EnergyType energyType, decimal quantityMWh)
        => new(Guid.NewGuid(), sellerId, buyerId, energyType, quantityMWh, 85m, Currency.EUR);

    private sealed class FakeTradeRepository : ITradeRepository
    {
        private readonly IReadOnlyList<Trade> _trades;
        public FakeTradeRepository(IReadOnlyList<Trade> trades) => _trades = trades;
        public Task AddAsync(Trade trade, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Trade?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_trades.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<Trade>> GetAllAsync(Guid? participantId = null, EnergyType? energyType = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = _trades.AsEnumerable();
            if (participantId.HasValue)
                query = query.Where(x => x.SellerId == participantId.Value || x.BuyerId == participantId.Value);
            if (energyType.HasValue)
                query = query.Where(x => x.EnergyType == energyType.Value);
            return Task.FromResult<IReadOnlyList<Trade>>(query.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        public Task<int> CountAsync(Guid? participantId = null, EnergyType? energyType = null, CancellationToken cancellationToken = default)
        {
            var query = _trades.AsEnumerable();
            if (participantId.HasValue)
                query = query.Where(x => x.SellerId == participantId.Value || x.BuyerId == participantId.Value);
            if (energyType.HasValue)
                query = query.Where(x => x.EnergyType == energyType.Value);
            return Task.FromResult(query.Count());
        }
    }
}
