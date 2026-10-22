using EnergyTrade.Application.Portfolios.Create;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Portfolios.Create;

public class CreatePortfolioCommandValidatorTests
{
    private readonly CreatePortfolioCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        var command = new CreatePortfolioCommand(
            Guid.NewGuid(),
            "Main Portfolio",
            Currency.EUR);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_HasValidationError()
    {
        var command = new CreatePortfolioCommand(
            Guid.Empty,
            "Main Portfolio",
            Currency.EUR);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreatePortfolioCommand.UserId));
    }

    [Fact]
    public async Task Validate_WithEmptyName_HasValidationError()
    {
        var command = new CreatePortfolioCommand(
            Guid.NewGuid(),
            "",
            Currency.EUR);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreatePortfolioCommand.Name));
    }
}