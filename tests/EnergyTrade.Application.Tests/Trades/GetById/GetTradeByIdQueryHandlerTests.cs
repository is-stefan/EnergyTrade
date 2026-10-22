using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Trades.GetById;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Trades.GetById;

public class GetTradeByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsSeller_ReturnsTrade()
    {
        var trade = CreateTrade();
        var repository = new FakeTradeRepository(trade);
        var handler = new GetTradeByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetTradeByIdQuery(trade.SellerId, trade.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(trade.Id, result.Id);
        Assert.Equal(trade.EnergyOfferId, result.EnergyOfferId);
        Assert.Equal(trade.SellerId, result.SellerId);
        Assert.Equal(trade.BuyerId, result.BuyerId);
        Assert.Equal(trade.EnergyType, result.EnergyType);
        Assert.Equal(trade.QuantityMWh, result.QuantityMWh);
        Assert.Equal(trade.PricePerMWh, result.PricePerMWh);
        Assert.Equal(trade.Currency, result.Currency);
        Assert.Equal(trade.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task Handle_WhenUserIsBuyer_ReturnsTrade()
    {
        var trade = CreateTrade();
        var repository = new FakeTradeRepository(trade);
        var handler = new GetTradeByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetTradeByIdQuery(trade.BuyerId, trade.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(trade.Id, result.Id);
    }

    [Fact]
    public async Task Handle_WhenTradeBelongsToAnotherUser_ThrowsForbiddenException()
    {
        var trade = CreateTrade();
        var repository = new FakeTradeRepository(trade);
        var handler = new GetTradeByIdQueryHandler(repository);
        var anotherUserId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                new GetTradeByIdQuery(anotherUserId, trade.Id),
                CancellationToken.None));

        Assert.Equal(
            "The trade does not belong to the authenticated user.",
            exception.Message);
    }

    [Fact]
    public async Task Handle_WhenTradeDoesNotExist_ReturnsNull()
    {
        var repository = new FakeTradeRepository(null);
        var handler = new GetTradeByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetTradeByIdQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }

    private static Trade CreateTrade()
    {
        return new Trade(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            85m,
            Currency.EUR);
    }

    private sealed class FakeTradeRepository : ITradeRepository
    {
        private readonly Trade? _trade;

        public FakeTradeRepository(Trade? trade)
        {
            _trade = trade;
        }

        public Task AddAsync(Trade trade, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<Trade?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_trade?.Id == id ? _trade : null);

        public Task<IReadOnlyList<Trade>> GetAllAsync(
            Guid? participantId = null,
            EnergyType? energyType = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Trade>>(Array.Empty<Trade>());

        public Task<int> CountAsync(
            Guid? participantId = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
