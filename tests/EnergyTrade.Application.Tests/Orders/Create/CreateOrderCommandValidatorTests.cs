using EnergyTrade.Application.Orders.Create;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Orders.Create;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        // Arrange
        var deliveryStart =
            DateTime.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(30);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
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
                nameof(CreateOrderCommand.UserId));
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
                nameof(CreateOrderCommand.PortfolioId));
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
                nameof(CreateOrderCommand.QuantityMWh));
    }

    [Fact]
    public async Task Validate_WithZeroPrice_HasValidationError()
    {
        var command = CreateValidCommand() with
        {
            MaxPricePerMWh = 0m
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateOrderCommand.MaxPricePerMWh));
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
                nameof(CreateOrderCommand.DeliveryStart));
    }

    private static CreateOrderCommand CreateValidCommand()
    {
        var deliveryStart =
            DateTime.UtcNow.AddDays(1);

        var deliveryEnd =
            deliveryStart.AddDays(30);

        return new CreateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }
}