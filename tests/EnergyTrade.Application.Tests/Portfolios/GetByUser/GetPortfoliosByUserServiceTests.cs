using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Portfolios.GetByUser;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.GetByUser;

public class GetPortfoliosByUserServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPortfoliosExist_ReturnsUserPortfolios()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var firstPortfolio = new Portfolio(
            userId,
            "Main Portfolio",
            Currency.EUR);

        var secondPortfolio = new Portfolio(
            userId,
            "Trading Portfolio",
            Currency.USD);

        var repository = new FakePortfolioRepository(
            [firstPortfolio, secondPortfolio]);

        var service = new GetPortfoliosByUserService(repository);

        // Act
        var result = await service.ExecuteAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            x =>
                x.Name == "Main Portfolio" &&
                x.BaseCurrency == Currency.EUR);

        Assert.Contains(
            result,
            x =>
                x.Name == "Trading Portfolio" &&
                x.BaseCurrency == Currency.USD);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPortfoliosExist_ReturnsEmptyList()
    {
        // Arrange
        var repository = new FakePortfolioRepository([]);

        var service = new GetPortfoliosByUserService(repository);

        // Act
        var result = await service.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.Empty(result);
    }

    private sealed class FakePortfolioRepository
        : IPortfolioRepository
    {
        private readonly List<Portfolio> _portfolios;

        public FakePortfolioRepository(
            List<Portfolio> portfolios)
        {
            _portfolios = portfolios;
        }

        public Task AddAsync(
            Portfolio portfolio,
            CancellationToken cancellationToken = default)
        {
            _portfolios.Add(portfolio);

            return Task.CompletedTask;
        }

        public Task<Portfolio?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var portfolio = _portfolios
                .FirstOrDefault(x => x.Id == id);

            return Task.FromResult(portfolio);
        }

        public Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Portfolio> portfolios = _portfolios
                .Where(x => x.UserId == userId)
                .ToList();

            return Task.FromResult(portfolios);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}