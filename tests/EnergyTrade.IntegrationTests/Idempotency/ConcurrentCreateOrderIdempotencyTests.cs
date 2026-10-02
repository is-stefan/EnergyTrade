using EnergyTrade.Api.ExceptionHandling;
using EnergyTrade.Application;
using EnergyTrade.Application.Orders.Create;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Infrastructure;
using EnergyTrade.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyTrade.IntegrationTests.Idempotency;

public class ConcurrentCreateOrderIdempotencyTests
{
    [Fact]
    public async Task SameIdempotencyKeyUsedConcurrently_CreatesOnlyOneOrder()
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

        var buyerId = Guid.NewGuid();

        var portfolio = new Portfolio(
            buyerId,
            "Idempotency Concurrency Portfolio",
            Currency.EUR);

        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(30);

        var request = new CreateOrderRequest(
            portfolio.Id,
            EnergyType.Solar,
            100m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        var idempotencyKey =
            $"create-order-{Guid.NewGuid()}";

        var scopedKey =
            $"{buyerId}:{idempotencyKey}";

        // Save portfolio required by CreateOrderService
        await using (var setupScope =
                     serviceProvider.CreateAsyncScope())
        {
            var dbContext =
                setupScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        try
        {
            // Two scopes = two separate DbContexts
            await using var scope1 =
                serviceProvider.CreateAsyncScope();

            await using var scope2 =
                serviceProvider.CreateAsyncScope();

            var service1 =
                scope1.ServiceProvider
                    .GetRequiredService<IdempotentCreateOrderService>();

            var service2 =
                scope2.ServiceProvider
                    .GetRequiredService<IdempotentCreateOrderService>();

            // Act - same user, same key, same operation, concurrently
            var results = await Task.WhenAll(
                service1.ExecuteAsync(
                    buyerId,
                    idempotencyKey,
                    request),
                service2.ExecuteAsync(
                    buyerId,
                    idempotencyKey,
                    request));

            // Assert - both callers receive the same order
            Assert.Equal(
                results[0].Id,
                results[1].Id);

            // Verify database state using a fresh DbContext
            await using var verificationScope =
                serviceProvider.CreateAsyncScope();

            var verificationContext =
                verificationScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            var orders =
                await verificationContext.Orders
                    .Where(x =>
                        x.BuyerId == buyerId &&
                        x.PortfolioId == portfolio.Id)
                    .ToListAsync();

            Assert.Single(orders);

            Assert.Equal(
                results[0].Id,
                orders[0].Id);

            var idempotencyRecords =
                await verificationContext.IdempotencyRecords
                    .Where(x =>
                        x.Key == scopedKey &&
                        x.Operation == "CreateOrder")
                    .ToListAsync();

            Assert.Single(idempotencyRecords);

            Assert.Equal(
                201,
                idempotencyRecords[0].StatusCode);
        }
        finally
        {
            // Cleanup
            await using var cleanupScope =
                serviceProvider.CreateAsyncScope();

            var cleanupContext =
                cleanupScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            var idempotencyRecords =
                await cleanupContext.IdempotencyRecords
                    .Where(x =>
                        x.Key == scopedKey &&
                        x.Operation == "CreateOrder")
                    .ToListAsync();

            cleanupContext.IdempotencyRecords.RemoveRange(
                idempotencyRecords);

            var orders =
                await cleanupContext.Orders
                    .Where(x =>
                        x.BuyerId == buyerId &&
                        x.PortfolioId == portfolio.Id)
                    .ToListAsync();

            cleanupContext.Orders.RemoveRange(
                orders);

            var savedPortfolio =
                await cleanupContext.Portfolios
                    .SingleOrDefaultAsync(
                        x => x.Id == portfolio.Id);

            if (savedPortfolio is not null)
            {
                cleanupContext.Portfolios.Remove(
                    savedPortfolio);
            }

            await cleanupContext.SaveChangesAsync();
        }
    }
}