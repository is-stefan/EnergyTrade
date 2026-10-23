using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Portfolios.Exposure;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.Exposure;

public class GetPortfolioExposureQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenPortfolioBelongsToUser_ReturnsExposure()
    {
        var userId = Guid.NewGuid();

        var portfolio = new Portfolio(
            userId,
            "Test Portfolio",
            Currency.EUR);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var exposureRepository =
            new FakePortfolioExposureRepository(125m);

        var handler =
            new GetPortfolioExposureQueryHandler(
                portfolioRepository,
                exposureRepository);

        var query =
            new GetPortfolioExposureQuery(
                userId,
                portfolio.Id);

        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        Assert.Equal(125m, result);
        Assert.Equal(
            portfolio.Id,
            exposureRepository.RequestedPortfolioId);
    }

    [Fact]
    public async Task Handle_WhenPortfolioDoesNotExist_ThrowsNotFoundException()
    {
        var portfolioRepository =
            new FakePortfolioRepository(null);

        var exposureRepository =
            new FakePortfolioExposureRepository(100m);

        var handler =
            new GetPortfolioExposureQueryHandler(
                portfolioRepository,
                exposureRepository);

        var query =
            new GetPortfolioExposureQuery(
                Guid.NewGuid(),
                Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                query,
                CancellationToken.None));

        Assert.Null(
            exposureRepository.RequestedPortfolioId);
    }

    [Fact]
    public async Task Handle_WhenPortfolioBelongsToAnotherUser_ThrowsForbiddenException()
    {
        var ownerId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var portfolio = new Portfolio(
            ownerId,
            "Test Portfolio",
            Currency.EUR);

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var exposureRepository =
            new FakePortfolioExposureRepository(100m);

        var handler =
            new GetPortfolioExposureQueryHandler(
                portfolioRepository,
                exposureRepository);

        var query =
            new GetPortfolioExposureQuery(
                authenticatedUserId,
                portfolio.Id);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                query,
                CancellationToken.None));

        Assert.Null(
            exposureRepository.RequestedPortfolioId);
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
            return Task.FromResult(_portfolio);
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

    private sealed class FakePortfolioExposureRepository
        : IPortfolioExposureRepository
    {
        private readonly decimal _exposure;

        public Guid? RequestedPortfolioId { get; private set; }

        public FakePortfolioExposureRepository(
            decimal exposure)
        {
            _exposure = exposure;
        }

        public Task<decimal> GetExposureAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            RequestedPortfolioId = portfolioId;

            return Task.FromResult(_exposure);
        }
    }
}