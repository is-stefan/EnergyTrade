using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Portfolios.Rename;
using EnergyTrade.Domain.Common.Exceptions;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.Rename;

public class RenamePortfolioCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPortfolioExistsAndBelongsToUser_RenamesPortfolio()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Old Name", Currency.EUR);
        var repository = new FakePortfolioRepository(portfolio);
        var handler = new RenamePortfolioCommandHandler(repository);
        var request = new RenamePortfolioRequest("New Name");

        var result = await handler.Handle(
            RenamePortfolioCommand.FromRequest(portfolio.UserId, portfolio.Id, request),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal("New Name", portfolio.Name);
        Assert.NotNull(portfolio.UpdatedAt);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPortfolioDoesNotExist_ReturnsFalse()
    {
        var repository = new FakePortfolioRepository(null);
        var handler = new RenamePortfolioCommandHandler(repository);
        var request = new RenamePortfolioRequest("New Name");

        var result = await handler.Handle(
            RenamePortfolioCommand.FromRequest(Guid.NewGuid(), Guid.NewGuid(), request),
            CancellationToken.None);

        Assert.False(result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPortfolioBelongsToAnotherUser_ThrowsForbiddenException()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Old Name", Currency.EUR);
        var repository = new FakePortfolioRepository(portfolio);
        var handler = new RenamePortfolioCommandHandler(repository);
        var request = new RenamePortfolioRequest("New Name");

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                RenamePortfolioCommand.FromRequest(Guid.NewGuid(), portfolio.Id, request),
                CancellationToken.None));

        Assert.Equal("The portfolio does not belong to the authenticated user.", exception.Message);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPortfolioIsClosed_ThrowsBusinessRuleException()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Old Name", Currency.EUR);
        portfolio.Close();
        var repository = new FakePortfolioRepository(portfolio);
        var handler = new RenamePortfolioCommandHandler(repository);
        var request = new RenamePortfolioRequest("New Name");

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(
                RenamePortfolioCommand.FromRequest(portfolio.UserId, portfolio.Id, request),
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
