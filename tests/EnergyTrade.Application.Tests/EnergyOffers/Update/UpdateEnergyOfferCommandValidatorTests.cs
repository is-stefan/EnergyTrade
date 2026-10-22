using EnergyTrade.Application.EnergyOffers.Update;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.Update;

public class UpdateEnergyOfferCommandValidatorTests
{
    private readonly UpdateEnergyOfferCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        var result =
            await _validator.ValidateAsync(CreateValidCommand());

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
                nameof(UpdateEnergyOfferCommand.UserId));
    }

    [Fact]
    public async Task Validate_WithEmptyEnergyOfferId_HasValidationError()
    {
        var command = CreateValidCommand() with
        {
            EnergyOfferId = Guid.Empty
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(UpdateEnergyOfferCommand.EnergyOfferId));
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
                nameof(UpdateEnergyOfferCommand.QuantityMWh));
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
                nameof(UpdateEnergyOfferCommand.PricePerMWh));
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
                nameof(UpdateEnergyOfferCommand.DeliveryStart));
    }

    private static UpdateEnergyOfferCommand CreateValidCommand()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddDays(30);

        return new UpdateEnergyOfferCommand(
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