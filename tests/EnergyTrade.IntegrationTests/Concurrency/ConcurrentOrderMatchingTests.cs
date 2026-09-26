using EnergyTrade.Api.ExceptionHandling;
using EnergyTrade.Application;
using EnergyTrade.Application.Orders.Match;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Infrastructure;
using EnergyTrade.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyTrade.IntegrationTests.Concurrency;

public class ConcurrentOrderMatchingTests
{
    [Fact]
    public async Task TwoOrdersMatchingSameOffer_OnlyOneOrderIsMatched()
    {
        // Arrange - configuration
        var apiProjectPath = Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "..",
                "..",
                "..",
                "..",
                "src",
                "EnergyTrade.Api"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile(
                "appsettings.json",
                optional: true)
            .AddJsonFile(
                "appsettings.Development.json",
                optional: true)
            .AddUserSecrets(
                typeof(GlobalExceptionHandler).Assembly,
                optional: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.AddApplication();
        services.AddInfrastructure(configuration);

        await using var serviceProvider =
            services.BuildServiceProvider();

        // Test users
        var sellerId = Guid.NewGuid();
        var buyer1Id = Guid.NewGuid();
        var buyer2Id = Guid.NewGuid();

        // Portfolios
        var sellerPortfolio = new Portfolio(
            sellerId,
            "Seller Concurrency Portfolio",
            Currency.EUR);

        var buyer1Portfolio = new Portfolio(
            buyer1Id,
            "Buyer 1 Concurrency Portfolio",
            Currency.EUR);

        var buyer2Portfolio = new Portfolio(
            buyer2Id,
            "Buyer 2 Concurrency Portfolio",
            Currency.EUR);

        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(30);

        // One offer which can satisfy either order,
        // but should only be consumed once.
        var energyOffer = new EnergyOffer(
            sellerId,
            sellerPortfolio.Id,
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var order1 = new Order(
            buyer1Id,
            buyer1Portfolio.Id,
            EnergyType.Solar,
            100m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var order2 = new Order(
            buyer2Id,
            buyer2Portfolio.Id,
            EnergyType.Solar,
            100m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Save initial data
        await using (var setupScope =
                     serviceProvider.CreateAsyncScope())
        {
            var dbContext =
                setupScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            dbContext.Portfolios.AddRange(
                sellerPortfolio,
                buyer1Portfolio,
                buyer2Portfolio);

            dbContext.EnergyOffers.Add(energyOffer);

            dbContext.Orders.AddRange(
                order1,
                order2);

            await dbContext.SaveChangesAsync();
        }

        try
        {
            // Two separate scopes = two separate DbContexts.
            await using var scope1 =
                serviceProvider.CreateAsyncScope();

            await using var scope2 =
                serviceProvider.CreateAsyncScope();

            var matchService1 =
                scope1.ServiceProvider
                    .GetRequiredService<MatchOrderService>();

            var matchService2 =
                scope2.ServiceProvider
                    .GetRequiredService<MatchOrderService>();

            // Act
            var results = await Task.WhenAll(
                matchService1.ExecuteAsync(order1.Id),
                matchService2.ExecuteAsync(order2.Id));

            // Assert - business result
            Assert.Equal(
                1,
                results.Count(result =>
                    result is { Matched: true }));

            Assert.Equal(
                1,
                results.Count(result =>
                    result is { Matched: false }));

            // Verify final database state using a fresh DbContext
            await using var verificationScope =
                serviceProvider.CreateAsyncScope();

            var verificationContext =
                verificationScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            var savedOffer =
                await verificationContext.EnergyOffers
                    .SingleAsync(
                        x => x.Id == energyOffer.Id);

            var savedOrder1 =
                await verificationContext.Orders
                    .SingleAsync(
                        x => x.Id == order1.Id);

            var savedOrder2 =
                await verificationContext.Orders
                    .SingleAsync(
                        x => x.Id == order2.Id);

            var trades =
                await verificationContext.Trades
                    .Where(
                        x => x.EnergyOfferId ==
                             energyOffer.Id)
                    .ToListAsync();

            var positions =
                await verificationContext.Positions
                    .Where(x =>
                        x.PortfolioId ==
                            sellerPortfolio.Id ||
                        x.PortfolioId ==
                            buyer1Portfolio.Id ||
                        x.PortfolioId ==
                            buyer2Portfolio.Id)
                    .ToListAsync();

            Assert.Equal(
                OfferStatus.Closed,
                savedOffer.Status);

            Assert.Equal(
                2,
                savedOffer.Version);

            Assert.Single(trades);

            Assert.Equal(
                1,
                new[]
                {
                    savedOrder1,
                    savedOrder2
                }.Count(
                    order =>
                        order.Status ==
                        OrderStatus.Filled));

            Assert.Equal(
                1,
                new[]
                {
                    savedOrder1,
                    savedOrder2
                }.Count(
                    order =>
                        order.Status ==
                        OrderStatus.Open));

            // One seller position and one winning buyer position.
            Assert.Equal(2, positions.Count);

            var sellerPosition =
                positions.Single(
                    x => x.PortfolioId ==
                         sellerPortfolio.Id);

            Assert.Equal(
                -100m,
                sellerPosition.QuantityMWh);

            var buyerPositions =
                positions.Where(
                    x => x.PortfolioId !=
                         sellerPortfolio.Id)
                    .ToList();

            Assert.Single(buyerPositions);

            Assert.Equal(
                100m,
                buyerPositions[0].QuantityMWh);
        }
        finally
        {
            // Cleanup with a fresh scope/context.
            await using var cleanupScope =
                serviceProvider.CreateAsyncScope();

            var cleanupContext =
                cleanupScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            var positions =
                await cleanupContext.Positions
                    .Where(x =>
                        x.PortfolioId ==
                            sellerPortfolio.Id ||
                        x.PortfolioId ==
                            buyer1Portfolio.Id ||
                        x.PortfolioId ==
                            buyer2Portfolio.Id)
                    .ToListAsync();

            cleanupContext.Positions.RemoveRange(
                positions);

            var trades =
                await cleanupContext.Trades
                    .Where(
                        x => x.EnergyOfferId ==
                             energyOffer.Id)
                    .ToListAsync();

            cleanupContext.Trades.RemoveRange(
                trades);

            var orders =
                await cleanupContext.Orders
                    .Where(x =>
                        x.Id == order1.Id ||
                        x.Id == order2.Id)
                    .ToListAsync();

            cleanupContext.Orders.RemoveRange(
                orders);

            var savedOffer =
                await cleanupContext.EnergyOffers
                    .SingleOrDefaultAsync(
                        x => x.Id ==
                             energyOffer.Id);

            if (savedOffer is not null)
            {
                cleanupContext.EnergyOffers.Remove(
                    savedOffer);
            }

            var portfolios =
                await cleanupContext.Portfolios
                    .Where(x =>
                        x.Id ==
                            sellerPortfolio.Id ||
                        x.Id ==
                            buyer1Portfolio.Id ||
                        x.Id ==
                            buyer2Portfolio.Id)
                    .ToListAsync();

            cleanupContext.Portfolios.RemoveRange(
                portfolios);

            await cleanupContext.SaveChangesAsync();
        }
    }
}