using EnergyTrade.Application.Auth.Login;

namespace EnergyTrade.Application.Tests.Auth.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        var command = new LoginCommand(
            "test@energytrade.com",
            "Password123!");

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_HasValidationError()
    {
        var command = new LoginCommand(
            "",
            "Password123!");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(LoginCommand.Email));
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_HasValidationError()
    {
        var command = new LoginCommand(
            "not-an-email",
            "Password123!");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(LoginCommand.Email));
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_HasValidationError()
    {
        var command = new LoginCommand(
            "test@energytrade.com",
            "");

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(LoginCommand.Password));
    }
}