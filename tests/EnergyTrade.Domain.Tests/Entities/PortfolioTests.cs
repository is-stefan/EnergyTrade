using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Tests.Entities;

public class PortfolioTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesActivePortfolio()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var portfolio = new Portfolio(
            userId,
            "Main Portfolio",
            Currency.EUR);

        // Assert
        Assert.NotEqual(Guid.Empty, portfolio.Id);
        Assert.Equal(userId, portfolio.UserId);
        Assert.Equal("Main Portfolio", portfolio.Name);
        Assert.Equal(Currency.EUR, portfolio.BaseCurrency);
        Assert.Equal(PortfolioStatus.Active, portfolio.Status);
        Assert.NotEqual(default, portfolio.CreatedAt);
        Assert.Null(portfolio.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Portfolio(
                Guid.Empty,
                "Main Portfolio",
                Currency.EUR));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Portfolio(
                Guid.NewGuid(),
                "",
                Currency.EUR));
    }

    [Fact]
    public void Rename_WhenPortfolioIsActive_ChangesName()
    {
        // Arrange
        var portfolio = CreatePortfolio();

        // Act
        portfolio.Rename("Trading Portfolio");

        // Assert
        Assert.Equal("Trading Portfolio", portfolio.Name);
        Assert.NotNull(portfolio.UpdatedAt);
    }

    [Fact]
    public void Close_WhenPortfolioIsActive_ClosesPortfolio()
    {
        // Arrange
        var portfolio = CreatePortfolio();

        // Act
        portfolio.Close();

        // Assert
        Assert.Equal(
            PortfolioStatus.Closed,
            portfolio.Status);

        Assert.NotNull(portfolio.UpdatedAt);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var portfolio = CreatePortfolio();
        portfolio.Close();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => portfolio.Close());
    }

    [Fact]
    public void Rename_WhenPortfolioIsClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var portfolio = CreatePortfolio();
        portfolio.Close();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => portfolio.Rename("New Name"));
    }

    private static Portfolio CreatePortfolio()
    {
        return new Portfolio(
            Guid.NewGuid(),
            "Main Portfolio",
            Currency.EUR);
    }
}