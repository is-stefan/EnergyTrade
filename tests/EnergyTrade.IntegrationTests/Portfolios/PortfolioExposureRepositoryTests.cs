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

namespace EnergyTrade.IntegrationTests.Portfolios;

public class PortfolioExposureRepositoryTests
{
    [Fact]
    public async Task GetExposureAsync_ReturnsSumOfPortfolioPositions()
    {
        var apiProjectPath = Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..", "..",
                "src", "EnergyTrade.Api"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
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

        var portfolio = new Portfolio(
            userId,
            "Exposure Test Portfolio",
            Currency.EUR);

        try
        {
            await using (var setupScope =
                         serviceProvider.CreateAsyncScope())
            {
                var dbContext =
                    setupScope.ServiceProvider
                        .GetRequiredService<EnergyTradeDbContext>();

                dbContext.Portfolios.Add(portfolio);

                await dbContext.SaveChangesAsync();

                var position1 = new Position(
                    portfolio.Id,
                    EnergyType.Solar);

                position1.ApplyTrade(100m);

                var position2 = new Position(
                    portfolio.Id,
                    EnergyType.Wind);

                position2.ApplyTrade(-40m);

                var position3 = new Position(
                    portfolio.Id,
                    EnergyType.Hydro);

                position3.ApplyTrade(25m);

                dbContext.Positions.AddRange(
                    position1,
                    position2,
                    position3);

                await dbContext.SaveChangesAsync();
            }

            await using var scope =
                serviceProvider.CreateAsyncScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<IPortfolioExposureRepository>();

            var exposure =
                await repository.GetExposureAsync(
                    portfolio.Id);

            Assert.Equal(85m, exposure);
        }
        finally
        {
            await using var cleanupScope =
                serviceProvider.CreateAsyncScope();

            var dbContext =
                cleanupScope.ServiceProvider
                    .GetRequiredService<EnergyTradeDbContext>();

            var positions =
                await dbContext.Positions
                    .Where(position =>
                        position.PortfolioId == portfolio.Id)
                    .ToListAsync();

            dbContext.Positions.RemoveRange(positions);

            var storedPortfolio =
                await dbContext.Portfolios
                    .FirstOrDefaultAsync(current =>
                        current.Id == portfolio.Id);

            if (storedPortfolio is not null)
            {
                dbContext.Portfolios.Remove(storedPortfolio);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}