using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Positions.GetByPortfolio;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Positions.GetByPortfolio;

public class GetPositionsByPortfolioServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPositionsExist_ReturnsPortfolioPositions()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();

        var solarPosition = new Position(
            portfolioId,
            EnergyType.Solar);

        solarPosition.ApplyTrade(100m);

        var windPosition = new Position(
            portfolioId,
            EnergyType.Wind);

        windPosition.ApplyTrade(-50m);

        var repository = new FakePositionRepository(
            [solarPosition, windPosition]);

        var service = new GetPositionsByPortfolioService(repository);

        // Act
        var result = await service.ExecuteAsync(portfolioId);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            x =>
                x.EnergyType == EnergyType.Solar &&
                x.QuantityMWh == 100m);

        Assert.Contains(
            result,
            x =>
                x.EnergyType == EnergyType.Wind &&
                x.QuantityMWh == -50m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPositionsExist_ReturnsEmptyList()
    {
        // Arrange
        var repository = new FakePositionRepository([]);

        var service = new GetPositionsByPortfolioService(repository);

        // Act
        var result = await service.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.Empty(result);
    }

    private sealed class FakePositionRepository
        : IPositionRepository
    {
        private readonly List<Position> _positions;

        public FakePositionRepository(
            List<Position> positions)
        {
            _positions = positions;
        }

        public Task<Position?> GetByPortfolioAndEnergyTypeAsync(
            Guid portfolioId,
            EnergyType energyType,
            CancellationToken cancellationToken = default)
        {
            var position = _positions.FirstOrDefault(x =>
                x.PortfolioId == portfolioId &&
                x.EnergyType == energyType);

            return Task.FromResult(position);
        }

        public Task<IReadOnlyList<Position>> GetByPortfolioIdAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Position> positions = _positions
                .Where(x => x.PortfolioId == portfolioId)
                .ToList();

            return Task.FromResult(positions);
        }

        public Task AddAsync(
            Position position,
            CancellationToken cancellationToken = default)
        {
            _positions.Add(position);

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}