namespace EnergyTrade.Domain.Entities;

public sealed class IdempotencyRecord
{
    public Guid Id { get; private set; }

    public string Key { get; private set; }

    public string Operation { get; private set; }

    public string Response { get; private set; }

    public int StatusCode { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private IdempotencyRecord()
    {
        Key = string.Empty;
        Operation = string.Empty;
        Response = string.Empty;
    }

    public IdempotencyRecord(
        string key,
        string operation,
        string response,
        int statusCode)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(
                "Idempotency key cannot be empty.",
                nameof(key));
        }

        if (string.IsNullOrWhiteSpace(operation))
        {
            throw new ArgumentException(
                "Operation cannot be empty.",
                nameof(operation));
        }

        if (string.IsNullOrWhiteSpace(response))
        {
            throw new ArgumentException(
                "Response cannot be empty.",
                nameof(response));
        }

        if (statusCode <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode));
        }

        Id = Guid.NewGuid();
        Key = key.Trim();
        Operation = operation.Trim();
        Response = response;
        StatusCode = statusCode;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}