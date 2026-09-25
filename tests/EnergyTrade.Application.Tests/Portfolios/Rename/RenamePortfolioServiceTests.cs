using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Portfolios.Rename;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.Rename;

public class RenamePortfolioServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPortfolioExists_RenamesPortfolio()
    {
        // Arrange
        var portfolio = new Portfolio(
            Guid.NewGuid(),
            "Old Name",
            Currency.EUR);

        var repository = new FakePortfolioRepository(portfolio);

        var service = new RenamePortfolioService(repository);

        var request = new RenamePortfolioRequest(
            "New Name");

        // Act
        var result = await service.ExecuteAsync(
            portfolio.Id,
            request);

        // Assert
        Assert.True(result);
        Assert.Equal("New Name", portfolio.Name);
        Assert.NotNull(portfolio.UpdatedAt);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new FakePortfolioRepository(null);

        var service = new RenamePortfolioService(repository);

        var request = new RenamePortfolioRequest(
            "New Name");

        // Act
        var result = await service.ExecuteAsync(
            Guid.NewGuid(),
            request);

        // Assert
        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioIsClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var portfolio = new Portfolio(
            Guid.NewGuid(),
            "Old Name",
            Currency.EUR);

        portfolio.Close();

        var repository = new FakePortfolioRepository(portfolio);

        var service = new RenamePortfolioService(repository);

        var request = new RenamePortfolioRequest(
            "New Name");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(
                portfolio.Id,
                request));

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