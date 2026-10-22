using EnergyTrade.Application.EnergyOffers.Create;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.Create;

public class CreateEnergyOfferCommandValidatorTests
{
    private readonly CreateEnergyOfferCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        var command = CreateValidCommand();

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_HasValidationError()
    {
        var command = CreateValidCommand() with
        {
            UserId = Guid.Empty
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateEnergyOfferCommand.UserId));
    }

    [Fact]
    public async Task Validate_WithEmptyPortfolioId_HasValidationError()
    {
        var command = CreateValidCommand() with
        {
            PortfolioId = Guid.Empty
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateEnergyOfferCommand.PortfolioId));
    }

    [Fact]
    public async Task Validate_WithZeroQuantity_HasValidationError()
    {
        var command = CreateValidCommand() with
        {
            QuantityMWh = 0m
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateEnergyOfferCommand.QuantityMWh));
    }

    [Fact]
    public async Task Validate_WithZeroPrice_HasValidationError()
    {
        var command = CreateValidCommand() with
        {
            PricePerMWh = 0m
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateEnergyOfferCommand.PricePerMWh));
    }

    [Fact]
    public async Task Validate_WithInvalidDeliveryRange_HasValidationError()
    {
        var command = CreateValidCommand();

        command = command with
        {
            DeliveryStart = command.DeliveryEnd
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateEnergyOfferCommand.DeliveryStart));
    }

    private static CreateEnergyOfferCommand CreateValidCommand()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddDays(30);

        return new CreateEnergyOfferCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            start,
            end);
    }
}