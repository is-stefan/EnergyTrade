using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Portfolios.GetById;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.GetById;

public class GetPortfolioByIdServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPortfolioExists_ReturnsPortfolio()
    {
        var portfolio = new Portfolio(
            Guid.NewGuid(),
            "Trading Portfolio",
            Currency.EUR);

        var repository = new FakePortfolioRepository(portfolio);

        var service = new GetPortfolioByIdService(repository);

        var result = await service.ExecuteAsync(portfolio.Id);

        Assert.NotNull(result);
        Assert.Equal(portfolio.Id, result.Id);
        Assert.Equal(portfolio.UserId, result.UserId);
        Assert.Equal(portfolio.Name, result.Name);
        Assert.Equal(portfolio.BaseCurrency, result.BaseCurrency);
        Assert.Equal(portfolio.Status, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotExist_ReturnsNull()
    {
        var repository = new FakePortfolioRepository(null);

        var service = new GetPortfolioByIdService(repository);

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Null(result);
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