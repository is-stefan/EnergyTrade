using EnergyTrade.Api.ExceptionHandling;
using EnergyTrade.Application;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Infrastructure;
using EnergyTrade.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyTrade.IntegrationTests.Trades;

public class TradeSummaryRepositoryTests
{
    [Fact]
    public async Task GetUserTradeSummaryAsync_ReturnsCorrectAggregates()
    {
        // Arrange
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

        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(configuration);

        await using var serviceProvider =
            services.BuildServiceProvider();

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var trade1 = new Trade(
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR);

        var trade2 = new Trade(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            EnergyType.Wind,
            50m,
            90m,
            Currency.EUR);

        var unrelatedTrade = new Trade(
            Guid.NewGuid(),
            otherUserId,
            Guid.NewGuid(),
            EnergyType.Solar,
            500m,
            200m,
            Currency.EUR);

        try
        {
            await using (var setupScope =
                         serviceProvider.CreateAsyncScope())
            {
                var dbContext =
                    setupScope.ServiceProvider
                        .GetRequiredService<EnergyTradeDbContext>();

                dbContext.Trades.AddRange(
                    trade1,
                    trade2,
                    unrelatedTrade);

                await dbContext.SaveChangesAsync();
            }

            await using var scope =
                serviceProvider.CreateAsyncScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<ITradeSummaryRepository>();

            // Act
            var result =
                await repository.GetUserTradeSummaryAsync(
                    userId,
                    Currency.EUR);

            // Assert
            Assert.Equal(
                2,
                result.TradeCount);

            Assert.Equal(
                150m,
                result.TotalMWh);

            Assert.Equal(
                85m,
                result.AveragePrice);

            Assert.Equal(
                12500m,
                result.TotalValue);
        }
        finally
        {
            await using var cleanupScope =
                serviceProvider.CreateAsyncScope();

            var dbContext =
                cleanupScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            var tradeIds = new[]
            {
                trade1.Id,
                trade2.Id,
                unrelatedTrade.Id
            };

            var trades =
                await dbContext.Trades
                    .Where(trade =>
                        tradeIds.Contains(trade.Id))
                    .ToListAsync();

            dbContext.Trades.RemoveRange(trades);

            await dbContext.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task GetUserTradeSummaryAsync_WithEnergyTypeFilter_ReturnsOnlyMatchingTrades()
    {
        // Arrange
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

        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(configuration);

        await using var serviceProvider =
            services.BuildServiceProvider();

        var userId = Guid.NewGuid();

        var solarTrade = new Trade(
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR);

        var windTrade = new Trade(
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            EnergyType.Wind,
            50m,
            90m,
            Currency.EUR);

        try
        {
            await using (var setupScope =
                         serviceProvider.CreateAsyncScope())
            {
                var dbContext =
                    setupScope.ServiceProvider
                        .GetRequiredService<EnergyTradeDbContext>();

                dbContext.Trades.AddRange(
                    solarTrade,
                    windTrade);

                await dbContext.SaveChangesAsync();
            }

            await using var scope =
                serviceProvider.CreateAsyncScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<ITradeSummaryRepository>();

            // Act
            var result =
                await repository.GetUserTradeSummaryAsync(
                    userId,
                    Currency.EUR,
                    EnergyType.Solar);

            // Assert
            Assert.Equal(
                1,
                result.TradeCount);

            Assert.Equal(
                100m,
                result.TotalMWh);

            Assert.Equal(
                80m,
                result.AveragePrice);

            Assert.Equal(
                8000m,
                result.TotalValue);
        }
        finally
        {
            await using var cleanupScope =
                serviceProvider.CreateAsyncScope();

            var dbContext =
                cleanupScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            var tradeIds = new[]
            {
                solarTrade.Id,
                windTrade.Id
            };

            var trades =
                await dbContext.Trades
                    .Where(trade =>
                        tradeIds.Contains(trade.Id))
                    .ToListAsync();

            dbContext.Trades.RemoveRange(trades);

            await dbContext.SaveChangesAsync();
        }
    }
}