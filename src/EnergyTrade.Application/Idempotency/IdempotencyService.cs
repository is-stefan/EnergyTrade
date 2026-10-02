using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
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

    public async Task<TResult?> GetResultAsync<TRequest, TResult>(
        string key,
        string operation,
        TRequest request,
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

        var requestHash = ComputeRequestHash(request);

        if (record.RequestHash != requestHash)
        {
            throw new IdempotencyConflictException(
                "The idempotency key has already been used with a different request.");
        }

        return JsonSerializer.Deserialize<TResult>(
            record.Response);
    }

    public async Task SaveResultAsync<TRequest, TResult>(
        string key,
        string operation,
        TRequest request,
        TResult result,
        int statusCode,
        CancellationToken cancellationToken = default)
    {
        var requestHash = ComputeRequestHash(request);

        var response = JsonSerializer.Serialize(result);

        var record = new IdempotencyRecord(
            key,
            operation,
            requestHash,
            response,
            statusCode);

        await _repository.AddAsync(
            record,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }

    private static string ComputeRequestHash<TRequest>(
        TRequest request)
    {
        var json = JsonSerializer.Serialize(request);

        var bytes = Encoding.UTF8.GetBytes(json);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}