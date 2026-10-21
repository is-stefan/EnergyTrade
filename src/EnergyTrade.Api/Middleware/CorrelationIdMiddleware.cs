namespace EnergyTrade.Api.Middleware;
using EnergyTrade.Api.Observability;

public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = CorrelationIdConstants.HeaderName;

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers.TryGetValue(
                CorrelationIdHeader,
                out var existingCorrelationId)
            && !string.IsNullOrWhiteSpace(existingCorrelationId)
                ? existingCorrelationId.ToString()
                : Guid.NewGuid().ToString();

        context.Items[CorrelationIdConstants.ItemKey] = correlationId;

        context.Response.Headers[CorrelationIdConstants.HeaderName] =
            correlationId;

        using (_logger.BeginScope(
            "CorrelationId:{CorrelationId}",
            correlationId))
        {
            await _next(context);
        }
    }
}