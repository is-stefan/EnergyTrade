using System.Text.Json;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Idempotency;

public sealed class IdempotencyService
{
    private readonly IIdempotencyRepository _repository;

    public IdempotencyService(
        IIdempotencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<T?> GetResultAsync<T>(
        string key,
        string operation,
        CancellationToken cancellationToken = default)
    {
        var record = await _repository.GetAsync(
            key,
            operation,
            cancellationToken);

        if (record is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(
            record.Response);
    }

    public async Task SaveResultAsync<T>(
        string key,
        string operation,
        T result,
        int statusCode,
        CancellationToken cancellationToken = default)
    {
        var response = JsonSerializer.Serialize(result);

        var record = new IdempotencyRecord(
            key,
            operation,
            response,
            statusCode);

        await _repository.AddAsync(
            record,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}