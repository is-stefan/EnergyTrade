using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Abstractions.Persistence;

public interface IIdempotencyRepository
{
    Task<IdempotencyRecord?> GetAsync(
        string key,
        string operation,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}