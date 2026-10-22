using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Portfolios.Close;
using EnergyTrade.Domain.Common.Exceptions;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.Close;

public class ClosePortfolioCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPortfolioExistsAndBelongsToUser_ClosesPortfolio()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Trading Portfolio", Currency.EUR);
        var repository = new FakePortfolioRepository(portfolio);
        var handler = new ClosePortfolioCommandHandler(repository);

        var result = await handler.Handle(
            new ClosePortfolioCommand(portfolio.UserId, portfolio.Id),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal(PortfolioStatus.Closed, portfolio.Status);
        Assert.NotNull(portfolio.UpdatedAt);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPortfolioDoesNotExist_ReturnsFalse()
    {
        var repository = new FakePortfolioRepository(null);
        var handler = new ClosePortfolioCommandHandler(repository);

        var result = await handler.Handle(
            new ClosePortfolioCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPortfolioBelongsToAnotherUser_ThrowsForbiddenException()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Trading Portfolio", Currency.EUR);
        var repository = new FakePortfolioRepository(portfolio);
        var handler = new ClosePortfolioCommandHandler(repository);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                new ClosePortfolioCommand(Guid.NewGuid(), portfolio.Id),
                CancellationToken.None));

        Assert.Equal("The portfolio does not belong to the authenticated user.", exception.Message);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPortfolioIsAlreadyClosed_ThrowsBusinessRuleException()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Trading Portfolio", Currency.EUR);
        portfolio.Close();
        var repository = new FakePortfolioRepository(portfolio);
        var handler = new ClosePortfolioCommandHandler(repository);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(
                new ClosePortfolioCommand(portfolio.UserId, portfolio.Id),
                CancellationToken.None));

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private sealed class FakePortfolioRepository : IPortfolioRepository
    {
        private readonly Portfolio? _portfolio;
        public int SaveChangesCallCount { get; private set; }
        public FakePortfolioRepository(Portfolio? portfolio) => _portfolio = portfolio;
        public Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_portfolio?.Id == id ? _portfolio : null);
        public Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Portfolio>>(Array.Empty<Portfolio>());
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        { SaveChangesCallCount++; return Task.CompletedTask; }
    }
}
