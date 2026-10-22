using EnergyTrade.Application.Common.Behaviors;
using FluentValidation;
using MediatR;

namespace EnergyTrade.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNoValidatorsExist_ExecutesNext()
    {
        // Arrange
        var validators =
            Array.Empty<IValidator<TestRequest>>();

        var behavior =
            new ValidationBehavior<TestRequest, string>(
                validators);

        var request = new TestRequest("valid");

        RequestHandlerDelegate<string> next =
            cancellationToken =>
                Task.FromResult("success");

        // Act
        var result = await behavior.Handle(
            request,
            next,
            CancellationToken.None);

        // Assert
        Assert.Equal("success", result);
    }

    [Fact]
    public async Task Handle_WhenValidationSucceeds_ExecutesNext()
    {
        // Arrange
        var validator = new TestRequestValidator();

        var behavior =
            new ValidationBehavior<TestRequest, string>(
                new[] { validator });

        var request = new TestRequest("valid");

        RequestHandlerDelegate<string> next =
            cancellationToken =>
                Task.FromResult("success");

        // Act
        var result = await behavior.Handle(
            request,
            next,
            CancellationToken.None);

        // Assert
        Assert.Equal("success", result);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var validator = new TestRequestValidator();

        var behavior =
            new ValidationBehavior<TestRequest, string>(
                new[] { validator });

        var request = new TestRequest("");

        RequestHandlerDelegate<string> next =
            cancellationToken =>
                Task.FromResult("success");

        // Act
        var exception =
            await Assert.ThrowsAsync<ValidationException>(
                () => behavior.Handle(
                    request,
                    next,
                    CancellationToken.None));

        // Assert
        Assert.Single(exception.Errors);

        var error = exception.Errors.Single();

        Assert.Equal(
            nameof(TestRequest.Name),
            error.PropertyName);
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsFail_AggregatesErrors()
    {
        // Arrange
        var validators =
            new IValidator<TestRequest>[]
            {
                new TestRequestValidator(),
                new AlwaysFailValidator()
            };

        var behavior =
            new ValidationBehavior<TestRequest, string>(
                validators);

        var request = new TestRequest("");

        RequestHandlerDelegate<string> next =
            cancellationToken =>
                Task.FromResult("success");

        // Act
        var exception =
            await Assert.ThrowsAsync<ValidationException>(
                () => behavior.Handle(
                    request,
                    next,
                    CancellationToken.None));

        // Assert
        Assert.Contains(
            exception.Errors,
            error =>
                error.PropertyName == nameof(TestRequest.Name));

        Assert.Contains(
            exception.Errors,
            error =>
                error.ErrorMessage == "Always fails.");
    }

    private sealed record TestRequest(string Name);

    private sealed class TestRequestValidator
        : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty();
        }
    }

    private sealed class AlwaysFailValidator
        : AbstractValidator<TestRequest>
    {
        public AlwaysFailValidator()
        {
            RuleFor(request => request.Name)
                .Must(_ => false)
                .WithMessage("Always fails.");
        }
    }
}