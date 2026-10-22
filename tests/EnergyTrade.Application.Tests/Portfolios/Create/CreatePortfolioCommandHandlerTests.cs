using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Portfolios.Create;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.Create;

public class CreatePortfolioCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_CreatesPortfolio()
    {
        var repository = new FakePortfolioRepository();
        var userId = Guid.NewGuid();
        var handler = new CreatePortfolioCommandHandler(repository);
        var request = new CreatePortfolioRequest("Main Portfolio", Currency.EUR);

        var result = await handler.Handle(
            CreatePortfolioCommand.FromRequest(userId, request),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Main Portfolio", result.Name);
        Assert.Equal(Currency.EUR, result.BaseCurrency);
        Assert.Equal(PortfolioStatus.Active, result.Status);
        Assert.NotNull(repository.AddedPortfolio);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ThrowsArgumentException()
    {
        var repository = new FakePortfolioRepository();
        var handler = new CreatePortfolioCommandHandler(repository);
        var request = new CreatePortfolioRequest("Main Portfolio", Currency.EUR);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(
                CreatePortfolioCommand.FromRequest(Guid.Empty, request),
                CancellationToken.None));

        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private sealed class FakePortfolioRepository : IPortfolioRepository
    {
        public Portfolio? AddedPortfolio { get; private set; }
        public int AddCallCount { get; private set; }
        public int SaveChangesCallCount { get; private set; }
        public Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
        { AddedPortfolio = portfolio; AddCallCount++; return Task.CompletedTask; }
        public Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Portfolio?>(null);
        public Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Portfolio>>(Array.Empty<Portfolio>());
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        { SaveChangesCallCount++; return Task.CompletedTask; }
    }
}
