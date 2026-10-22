using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Portfolios.GetById;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.GetById;

public class GetPortfolioByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenPortfolioExistsAndBelongsToUser_ReturnsPortfolio()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Trading Portfolio", Currency.EUR);
        var handler = new GetPortfolioByIdQueryHandler(new FakePortfolioRepository(portfolio));

        var result = await handler.Handle(
            new GetPortfolioByIdQuery(portfolio.UserId, portfolio.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(portfolio.Id, result.Id);
        Assert.Equal(portfolio.UserId, result.UserId);
        Assert.Equal(portfolio.Name, result.Name);
        Assert.Equal(portfolio.BaseCurrency, result.BaseCurrency);
        Assert.Equal(portfolio.Status, result.Status);
    }

    [Fact]
    public async Task Handle_WhenPortfolioDoesNotExist_ReturnsNull()
    {
        var handler = new GetPortfolioByIdQueryHandler(new FakePortfolioRepository(null));
        var result = await handler.Handle(
            new GetPortfolioByIdQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenPortfolioBelongsToAnotherUser_ThrowsForbiddenException()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Trading Portfolio", Currency.EUR);
        var handler = new GetPortfolioByIdQueryHandler(new FakePortfolioRepository(portfolio));

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                new GetPortfolioByIdQuery(Guid.NewGuid(), portfolio.Id),
                CancellationToken.None));

        Assert.Equal("The portfolio does not belong to the authenticated user.", exception.Message);
    }

    private sealed class FakePortfolioRepository : IPortfolioRepository
    {
        private readonly Portfolio? _portfolio;
        public FakePortfolioRepository(Portfolio? portfolio) => _portfolio = portfolio;
        public Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_portfolio?.Id == id ? _portfolio : null);
        public Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Portfolio>>(Array.Empty<Portfolio>());
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
