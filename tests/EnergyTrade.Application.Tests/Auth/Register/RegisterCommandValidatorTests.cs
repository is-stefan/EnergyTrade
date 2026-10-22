using EnergyTrade.Application.Auth.Register;

namespace EnergyTrade.Application.Tests.Auth.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        var command = new RegisterCommand(
            "test@energytrade.com",
            "Password123!");

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_HasValidationError()
    {
        var command = new RegisterCommand(
            "",
            "Password123!");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(RegisterCommand.Email));
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_HasValidationError()
    {
        var command = new RegisterCommand(
            "invalid-email",
            "Password123!");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(RegisterCommand.Email));
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_HasValidationError()
    {
        var command = new RegisterCommand(
            "test@energytrade.com",
            "");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(RegisterCommand.Password));
    }
}