using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrade.Infrastructure.Persistence.Repositories;

public sealed class IdempotencyRepository
    : IIdempotencyRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public IdempotencyRepository(
        EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdempotencyRecord?> GetAsync(
        string key,
        string operation,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Key == key &&
                     x.Operation == operation,
                cancellationToken);
    }

    public async Task AddAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.IdempotencyRecords.AddAsync(
            record,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}