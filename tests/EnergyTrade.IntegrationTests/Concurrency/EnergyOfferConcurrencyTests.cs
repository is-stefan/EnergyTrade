using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EnergyTrade.Api.ExceptionHandling;

namespace EnergyTrade.IntegrationTests.Concurrency;

public class EnergyOfferConcurrencyTests
{
    [Fact]
    public async Task SavingSameEnergyOfferFromTwoContexts_ThrowsConcurrencyException()
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

        var connectionString =
            configuration.GetConnectionString("OracleDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'OracleDatabase' was not found.");

        var options =
            new DbContextOptionsBuilder<EnergyTradeDbContext>()
                .UseOracle(connectionString)
                .Options;

        var sellerId = Guid.NewGuid();

        var portfolio = new Portfolio(
            sellerId,
            "Concurrency Test Portfolio",
            Currency.EUR);

        var deliveryStart =
            DateTimeOffset.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(30);

        var offer = new EnergyOffer(
            sellerId,
            portfolio.Id,
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        Guid offerId;
        Guid portfolioId;

        await using (var setupContext =
                     new EnergyTradeDbContext(options))
        {
            setupContext.Portfolios.Add(portfolio);
            setupContext.EnergyOffers.Add(offer);

            await setupContext.SaveChangesAsync();

            offerId = offer.Id;
            portfolioId = portfolio.Id;
        }

        // Load the same offer in two separate DbContexts
        await using var context1 =
            new EnergyTradeDbContext(options);

        await using var context2 =
            new EnergyTradeDbContext(options);

        var offer1 = await context1.EnergyOffers
            .SingleAsync(x => x.Id == offerId);

        var offer2 = await context2.EnergyOffers
            .SingleAsync(x => x.Id == offerId);

        Assert.Equal(1, offer1.Version);
        Assert.Equal(1, offer2.Version);

        // First context modifies and saves successfully
        offer1.Close();

        await context1.SaveChangesAsync();

        Assert.Equal(2, offer1.Version);

        // Second context still works with the old database version
        offer2.Close();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => context2.SaveChangesAsync());

        // Cleanup
        await using var cleanupContext =
            new EnergyTradeDbContext(options);

        var savedOffer = await cleanupContext.EnergyOffers
            .SingleAsync(x => x.Id == offerId);

        cleanupContext.EnergyOffers.Remove(savedOffer);

        var savedPortfolio = await cleanupContext.Portfolios
            .SingleAsync(x => x.Id == portfolioId);

        cleanupContext.Portfolios.Remove(savedPortfolio);

        await cleanupContext.SaveChangesAsync();
    }
}