using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Portfolios.GetByUser;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.GetByUser;

public class GetPortfoliosByUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenPortfoliosExist_ReturnsUserPortfolios()
    {
        var userId = Guid.NewGuid();
        var firstPortfolio = new Portfolio(userId, "Main Portfolio", Currency.EUR);
        var secondPortfolio = new Portfolio(userId, "Trading Portfolio", Currency.USD);
        var handler = new GetPortfoliosByUserQueryHandler(
            new FakePortfolioRepository([firstPortfolio, secondPortfolio]));

        var result = await handler.Handle(
            new GetPortfoliosByUserQuery(userId),
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Main Portfolio" && x.BaseCurrency == Currency.EUR);
        Assert.Contains(result, x => x.Name == "Trading Portfolio" && x.BaseCurrency == Currency.USD);
    }

    [Fact]
    public async Task Handle_WhenNoPortfoliosExist_ReturnsEmptyList()
    {
        var handler = new GetPortfoliosByUserQueryHandler(new FakePortfolioRepository([]));
        var result = await handler.Handle(
            new GetPortfoliosByUserQuery(Guid.NewGuid()),
            CancellationToken.None);
        Assert.Empty(result);
    }

    private sealed class FakePortfolioRepository : IPortfolioRepository
    {
        private readonly List<Portfolio> _portfolios;
        public FakePortfolioRepository(List<Portfolio> portfolios) => _portfolios = portfolios;
        public Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
        { _portfolios.Add(portfolio); return Task.CompletedTask; }
        public Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_portfolios.FirstOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Portfolio>>(_portfolios.Where(x => x.UserId == userId).ToList());
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
