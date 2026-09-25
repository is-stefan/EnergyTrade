using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Portfolios.Create;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.Create;

public class CreatePortfolioServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesPortfolio()
    {
        // Arrange
        var repository = new FakePortfolioRepository();

        var service = new CreatePortfolioService(repository);

        var request = new CreatePortfolioRequest(
            Guid.NewGuid(),
            "Main Portfolio",
            Currency.EUR);

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.UserId, result.UserId);
        Assert.Equal("Main Portfolio", result.Name);
        Assert.Equal(Currency.EUR, result.BaseCurrency);
        Assert.Equal(PortfolioStatus.Active, result.Status);

        Assert.NotNull(repository.AddedPortfolio);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakePortfolioRepository();

        var service = new CreatePortfolioService(repository);

        var request = new CreatePortfolioRequest(
            Guid.Empty,
            "Main Portfolio",
            Currency.EUR);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private sealed class FakePortfolioRepository
        : IPortfolioRepository
    {
        public Portfolio? AddedPortfolio { get; private set; }

        public int AddCallCount { get; private set; }

        public int SaveChangesCallCount { get; private set; }

        public Task AddAsync(
            Portfolio portfolio,
            CancellationToken cancellationToken = default)
        {
            AddedPortfolio = portfolio;
            AddCallCount++;

            return Task.CompletedTask;
        }

        public Task<Portfolio?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
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