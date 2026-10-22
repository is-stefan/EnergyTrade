using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Domain.Common.Exceptions;
using EnergyTrade.Api.Observability;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ValidationException =>
                StatusCodes.Status400BadRequest,

            UnauthorizedException =>
                StatusCodes.Status401Unauthorized,

            ForbiddenException =>
                StatusCodes.Status403Forbidden,

            NotFoundException =>
                StatusCodes.Status404NotFound,

            ConflictException =>
                StatusCodes.Status409Conflict,

            IdempotencyConflictException =>
                StatusCodes.Status409Conflict,

            BusinessRuleException =>
                StatusCodes.Status409Conflict,

            ArgumentException =>
                StatusCodes.Status400BadRequest,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        var title = exception switch
        {
            ValidationException =>
                "Validation failed",

            _ => statusCode switch
            {
                StatusCodes.Status400BadRequest =>
                    "Invalid request",

                StatusCodes.Status401Unauthorized =>
                    "Unauthorized",

                StatusCodes.Status403Forbidden =>
                    "Forbidden",

                StatusCodes.Status404NotFound =>
                    "Not found",

                StatusCodes.Status409Conflict =>
                    "Conflict",

                _ =>
                    "Internal server error"
            }
        };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred while processing {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "Request failed with status {StatusCode}: {ExceptionMessage}. TraceId: {TraceId}",
                statusCode,
                exception.Message,
                httpContext.TraceIdentifier);
        }

        var detail = exception switch
        {
            ValidationException =>
                "One or more validation errors occurred.",

            _ when statusCode ==
                StatusCodes.Status500InternalServerError =>
                "An unexpected error occurred.",

            _ =>
                exception.Message
        };

        var correlationId =
            httpContext.Items.TryGetValue(
                CorrelationIdConstants.ItemKey,
                out var value)
                ? value?.ToString()
                : null;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            problemDetails.Extensions["correlationId"] =
                correlationId;
        }

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] =
                validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .Distinct()
                            .ToArray());
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}