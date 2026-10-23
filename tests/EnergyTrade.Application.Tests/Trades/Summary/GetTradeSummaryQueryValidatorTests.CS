using EnergyTrade.Application.Trades.Summary;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Trades.Summary;

public class GetTradeSummaryQueryValidatorTests
{
    private readonly GetTradeSummaryQueryValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidQuery_IsValid()
    {
        var query = new GetTradeSummaryQuery(
            Guid.NewGuid(),
            Currency.EUR,
            EnergyType.Solar);

        var result = await _validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithoutEnergyType_IsValid()
    {
        var query = new GetTradeSummaryQuery(
            Guid.NewGuid(),
            Currency.EUR);

        var result = await _validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_HasValidationError()
    {
        var query = new GetTradeSummaryQuery(
            Guid.Empty,
            Currency.EUR);

        var result = await _validator.ValidateAsync(query);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(GetTradeSummaryQuery.UserId));
    }

    [Fact]
    public async Task Validate_WithInvalidCurrency_HasValidationError()
    {
        var query = new GetTradeSummaryQuery(
            Guid.NewGuid(),
            (Currency)999);

        var result = await _validator.ValidateAsync(query);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(GetTradeSummaryQuery.Currency));
    }

    [Fact]
    public async Task Validate_WithInvalidEnergyType_HasValidationError()
    {
        var query = new GetTradeSummaryQuery(
            Guid.NewGuid(),
            Currency.EUR,
            (EnergyType)999);

        var result = await _validator.ValidateAsync(query);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(GetTradeSummaryQuery.EnergyType));
    }
}