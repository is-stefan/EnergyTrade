using EnergyTrade.Application.Portfolios.Rename;

namespace EnergyTrade.Application.Tests.Portfolios.Rename;

public class RenamePortfolioCommandValidatorTests
{
    private readonly RenamePortfolioCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        var command = new RenamePortfolioCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Updated Portfolio");

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_HasValidationError()
    {
        var command = new RenamePortfolioCommand(
            Guid.Empty,
            Guid.NewGuid(),
            "Updated Portfolio");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(RenamePortfolioCommand.UserId));
    }

    [Fact]
    public async Task Validate_WithEmptyPortfolioId_HasValidationError()
    {
        var command = new RenamePortfolioCommand(
            Guid.NewGuid(),
            Guid.Empty,
            "Updated Portfolio");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(RenamePortfolioCommand.PortfolioId));
    }

    [Fact]
    public async Task Validate_WithEmptyName_HasValidationError()
    {
        var command = new RenamePortfolioCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(RenamePortfolioCommand.Name));
    }
}