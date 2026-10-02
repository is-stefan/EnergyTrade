using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Positions.GetByPortfolio;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Positions.GetByPortfolio;

public class GetPositionsByPortfolioServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPositionsExistAndPortfolioBelongsToUser_ReturnsPortfolioPositions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var portfolio = new Portfolio(
            userId,
            "Trading Portfolio",
            Currency.EUR);

        var solarPosition = new Position(
            portfolio.Id,
            EnergyType.Solar);

        solarPosition.ApplyTrade(100m);

        var windPosition = new Position(
            portfolio.Id,
            EnergyType.Wind);

        windPosition.ApplyTrade(-50m);

        var positionRepository = new FakePositionRepository(
            [solarPosition, windPosition]);

        var portfolioRepository = new FakePortfolioRepository(
            portfolio);

        var service = new GetPositionsByPortfolioService(
            positionRepository,
            portfolioRepository);

        // Act
        var result = await service.ExecuteAsync(
            userId,
            portfolio.Id);

        // Assert
        Assert.NotNull(result);
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
    public async Task ExecuteAsync_WhenPortfolioHasNoPositions_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var portfolio = new Portfolio(
            userId,
            "Trading Portfolio",
            Currency.EUR);

        var positionRepository = new FakePositionRepository([]);

        var portfolioRepository = new FakePortfolioRepository(
            portfolio);

        var service = new GetPositionsByPortfolioService(
            positionRepository,
            portfolioRepository);

        // Act
        var result = await service.ExecuteAsync(
            userId,
            portfolio.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotExist_ReturnsNull()
    {
        // Arrange
        var positionRepository = new FakePositionRepository([]);

        var portfolioRepository = new FakePortfolioRepository(
            null);

        var service = new GetPositionsByPortfolioService(
            positionRepository,
            portfolioRepository);

        // Act
        var result = await service.ExecuteAsync(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioBelongsToAnotherUser_ThrowsForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var portfolio = new Portfolio(
            ownerId,
            "Trading Portfolio",
            Currency.EUR);

        var positionRepository = new FakePositionRepository([]);

        var portfolioRepository = new FakePortfolioRepository(
            portfolio);

        var service = new GetPositionsByPortfolioService(
            positionRepository,
            portfolioRepository);

        // Act
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => service.ExecuteAsync(
                anotherUserId,
                portfolio.Id));

        // Assert
        Assert.Equal(
            "The portfolio does not belong to the authenticated user.",
            exception.Message);
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

    private sealed class FakePortfolioRepository
        : IPortfolioRepository
    {
        private readonly Portfolio? _portfolio;

        public FakePortfolioRepository(
            Portfolio? portfolio)
        {
            _portfolio = portfolio;
        }

        public Task AddAsync(
            Portfolio portfolio,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Portfolio?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (_portfolio?.Id == id)
            {
                return Task.FromResult<Portfolio?>(_portfolio);
            }

            return Task.FromResult<Portfolio?>(null);
        }

        public Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (_portfolio?.UserId == userId)
            {
                return Task.FromResult<IReadOnlyList<Portfolio>>(
                    [_portfolio]);
            }

            return Task.FromResult<IReadOnlyList<Portfolio>>(
                Array.Empty<Portfolio>());
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}