using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Trades.GetById;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Trades.GetById;

public class GetTradeByIdServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenTradeExists_ReturnsTrade()
    {
        // Arrange
        var trade = CreateTrade();
        var repository = new FakeTradeRepository(trade);
        var service = new GetTradeByIdService(repository);

        // Act
        var result = await service.ExecuteAsync(trade.Id);

        // Assert
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
    public async Task ExecuteAsync_WhenTradeDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeTradeRepository(null);
        var service = new GetTradeByIdService(repository);

        // Act
        var result = await service.ExecuteAsync(Guid.NewGuid());

        // Assert
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

    private sealed class FakeTradeRepository
        : ITradeRepository
    {
        private readonly Trade? _trade;

        public FakeTradeRepository(Trade? trade)
        {
            _trade = trade;
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
            if (_trade?.Id == id)
            {
                return Task.FromResult<Trade?>(_trade);
            }

            return Task.FromResult<Trade?>(null);
        }
    }
}