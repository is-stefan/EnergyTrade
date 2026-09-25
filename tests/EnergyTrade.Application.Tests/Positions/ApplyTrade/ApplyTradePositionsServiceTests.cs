using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Positions.ApplyTrade;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Positions.ApplyTrade;

public class ApplyTradeToPositionsServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPositionsDoNotExist_CreatesBothPositions()
    {
        // Arrange
        var repository = new FakePositionRepository();

        var service = new ApplyTradeToPositionsService(repository);

        var trade = CreateTrade();

        var buyerPortfolioId = Guid.NewGuid();
        var sellerPortfolioId = Guid.NewGuid();

        // Act
        await service.ExecuteAsync(
            trade,
            buyerPortfolioId,
            sellerPortfolioId);

        // Assert
        Assert.Equal(2, repository.Positions.Count);

        var buyerPosition = repository.Positions
            .Single(x => x.PortfolioId == buyerPortfolioId);

        var sellerPosition = repository.Positions
            .Single(x => x.PortfolioId == sellerPortfolioId);

        Assert.Equal(
            EnergyType.Solar,
            buyerPosition.EnergyType);

        Assert.Equal(
            EnergyType.Solar,
            sellerPosition.EnergyType);

        Assert.Equal(
            100m,
            buyerPosition.QuantityMWh);

        Assert.Equal(
            -100m,
            sellerPosition.QuantityMWh);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPositionsExist_UpdatesExistingPositions()
    {
        // Arrange
        var repository = new FakePositionRepository();

        var trade = CreateTrade();

        var buyerPortfolioId = Guid.NewGuid();
        var sellerPortfolioId = Guid.NewGuid();

        var buyerPosition = new Position(
            buyerPortfolioId,
            trade.EnergyType);

        buyerPosition.ApplyTrade(50m);

        var sellerPosition = new Position(
            sellerPortfolioId,
            trade.EnergyType);

        sellerPosition.ApplyTrade(-20m);

        repository.Positions.Add(buyerPosition);
        repository.Positions.Add(sellerPosition);

        var service = new ApplyTradeToPositionsService(repository);

        // Act
        await service.ExecuteAsync(
            trade,
            buyerPortfolioId,
            sellerPortfolioId);

        // Assert
        Assert.Equal(2, repository.Positions.Count);

        Assert.Equal(
            150m,
            buyerPosition.QuantityMWh);

        Assert.Equal(
            -120m,
            sellerPosition.QuantityMWh);
    }

    private static Trade CreateTrade()
    {
        return new Trade(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR);
    }

    private sealed class FakePositionRepository
        : IPositionRepository
    {
        public List<Position> Positions { get; } = [];

        public Task<Position?> GetByPortfolioAndEnergyTypeAsync(
            Guid portfolioId,
            EnergyType energyType,
            CancellationToken cancellationToken = default)
        {
            var position = Positions.FirstOrDefault(x =>
                x.PortfolioId == portfolioId &&
                x.EnergyType == energyType);

            return Task.FromResult(position);
        }

        public Task<IReadOnlyList<Position>> GetByPortfolioIdAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Position> positions = Positions
                .Where(x => x.PortfolioId == portfolioId)
                .ToList();

            return Task.FromResult(positions);
        }

        public Task AddAsync(
            Position position,
            CancellationToken cancellationToken = default)
        {
            Positions.Add(position);

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}