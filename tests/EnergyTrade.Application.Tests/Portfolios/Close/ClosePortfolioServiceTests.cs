using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Portfolios.Close;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.Close;

public class ClosePortfolioServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPortfolioExists_ClosesPortfolio()
    {
        // Arrange
        var portfolio = new Portfolio(
            Guid.NewGuid(),
            "Trading Portfolio",
            Currency.EUR);

        var repository = new FakePortfolioRepository(portfolio);

        var service = new ClosePortfolioService(repository);

        // Act
        var result = await service.ExecuteAsync(portfolio.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(PortfolioStatus.Closed, portfolio.Status);
        Assert.NotNull(portfolio.UpdatedAt);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new FakePortfolioRepository(null);

        var service = new ClosePortfolioService(repository);

        // Act
        var result = await service.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioIsAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var portfolio = new Portfolio(
            Guid.NewGuid(),
            "Trading Portfolio",
            Currency.EUR);

        portfolio.Close();

        var repository = new FakePortfolioRepository(portfolio);

        var service = new ClosePortfolioService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(portfolio.Id));

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private sealed class FakePortfolioRepository
        : IPortfolioRepository
    {
        private readonly Portfolio? _portfolio;

        public int SaveChangesCallCount { get; private set; }

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
            return Task.FromResult<IReadOnlyList<Portfolio>>(
                Array.Empty<Portfolio>());
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }
}